using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Models;
using TitanFitness.Application.Members.Queries.GetMemberDirectory;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Members;

public sealed class MemberDirectoryReadService
    : IMemberDirectoryReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public MemberDirectoryReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MemberDirectoryItemDto>> GetAsync(
        string? search,
        IReadOnlyCollection<int>? branchIds,
        IReadOnlyCollection<MembershipStatus>? statuses,
        string? sortBy,
        string? sortDirection,
        DateOnly asOfDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Members
            .AsNoTracking()
            .AsQueryable();

        if (branchIds is not null && branchIds.Count > 0)
        {
            var branchIdList = branchIds.Distinct().ToList();

            query = query.Where(member =>
                branchIdList.Contains(member.HomeBranchId));
        }

        var members = await query
            .Select(member => new
            {
                member.MemberId,
                member.FullName,
                member.MembershipNumber,
                member.HomeBranchId,
                BranchName = _dbContext.Branches
                    .Where(branch =>
                        branch.BranchId == member.HomeBranchId)
                    .Select(branch => branch.BranchName)
                    .FirstOrDefault()!,
                Memberships = _dbContext.Memberships
                    .Where(membership =>
                        membership.MemberId == member.MemberId)
                    .Select(membership => new
                    {
                        membership.MembershipId,
                        membership.StartDate,
                        membership.EndDate,
                        membership.Status,
                        IsFrozen = _dbContext.Freezes.Any(
                            freeze =>
                                freeze.MembershipId ==
                                    membership.MembershipId &&
                                freeze.StartDate <= asOfDate &&
                                asOfDate < freeze.EndDate)
                    })
                    .ToList(),
                LastVisit = _dbContext.CheckIns
                    .Where(checkIn =>
                        checkIn.MemberId == member.MemberId &&
                        checkIn.Result == CheckInResult.Admitted)
                    .OrderByDescending(checkIn =>
                        checkIn.CheckInDateTime)
                    .Select(checkIn =>
                        (DateTime?)checkIn.CheckInDateTime)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var freezeCounts = await _dbContext.Freezes
            .AsNoTracking()
            .GroupBy(freeze => freeze.MembershipId)
            .Select(group => new
            {
                MembershipId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                x => x.MembershipId,
                x => x.Count,
                cancellationToken);

        var maximumFreezes = await _dbContext.Memberships
            .AsNoTracking()
            .Select(membership => new
            {
                membership.MembershipId,
                Maximum = membership.AgreedTerms.MaximumNumberOfFreezes
            })
            .ToDictionaryAsync(
                x => x.MembershipId,
                x => x.Maximum,
                cancellationToken);

        var rows = members
            .Select(member =>
            {
                var data = member.Memberships
                    .Select(x => new MembershipStatusData(
                        x.MembershipId,
                        x.StartDate,
                        x.EndDate,
                        x.Status,
                        x.IsFrozen))
                    .ToList();

                var status = GetMemberStatus(data, asOfDate);

                var covering = data
                    .Where(x =>
                        x.Status != MembershipStatus.Cancelled &&
                        x.StartDate <= asOfDate &&
                        asOfDate < x.EndDate)
                    .OrderByDescending(x => x.StartDate)
                    .FirstOrDefault();

                int? freezesRemaining = null;

                if (covering is not null)
                {
                    var used = freezeCounts.GetValueOrDefault(
                        covering.MembershipId);

                    var maximum = maximumFreezes.GetValueOrDefault(
                        covering.MembershipId);

                    freezesRemaining = Math.Max(0, maximum - used);
                }

                return new MemberDirectoryItemDto(
                    member.MemberId,
                    member.FullName,
                    member.MembershipNumber,
                    status,
                    member.HomeBranchId,
                    member.BranchName,
                    member.LastVisit,
                    covering?.MembershipId,
                    freezesRemaining);
            })
            .ToList();

        if (statuses is not null && statuses.Count > 0)
        {
            var statusSet = statuses.ToHashSet();

            rows = rows
                .Where(x =>
                    x.Status.HasValue &&
                    statusSet.Contains(x.Status.Value))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var idTerm = term.TrimStart('#');

            if (idTerm.Length == 0)
            {
                idTerm = term;
            }

            var now = DateTime.Now;

            rows = rows
                .Where(x =>
                    Contains(x.FullName, term) ||
                    Contains(x.MembershipNumber, idTerm) ||
                    Contains(x.Status?.ToString(), term) ||
                    Contains(x.BranchName, term) ||
                    Contains(LastVisitText(x.LastVisit, now), term))
                .ToList();
        }

        var sorted = Sort(rows, sortBy, sortDirection).ToList();

        var totalCount = sorted.Count;

        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<MemberDirectoryItemDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IOrderedEnumerable<MemberDirectoryItemDto> Sort(
        IEnumerable<MemberDirectoryItemDto> rows,
        string? sortBy,
        string? sortDirection)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;

        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        var key = sortBy?.Trim().ToLowerInvariant();

        IOrderedEnumerable<MemberDirectoryItemDto> ordered = key switch
        {
            "number" => descending
                ? rows.OrderByDescending(x => x.MembershipNumber, comparer)
                : rows.OrderBy(x => x.MembershipNumber, comparer),

            "status" => descending
                ? rows.OrderByDescending(x => x.Status?.ToString() ?? string.Empty, comparer)
                : rows.OrderBy(x => x.Status?.ToString() ?? string.Empty, comparer),

            "branch" => descending
                ? rows.OrderByDescending(x => x.BranchName, comparer)
                : rows.OrderBy(x => x.BranchName, comparer),

            "lastvisit" => descending
                ? rows.OrderByDescending(x => x.LastVisit ?? DateTime.MinValue)
                : rows.OrderBy(x => x.LastVisit ?? DateTime.MinValue),

            _ => descending
                ? rows.OrderByDescending(x => x.FullName, comparer)
                : rows.OrderBy(x => x.FullName, comparer)
        };

        return ordered.ThenBy(x => x.FullName, comparer);
    }

    private static string LastVisitText(
        DateTime? lastVisit,
        DateTime now)
    {
        if (!lastVisit.HasValue)
        {
            return string.Empty;
        }

        var visit = lastVisit.Value;

        if (visit.Date == now.Date)
        {
            return "Today, " +
                   visit.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        if (visit.Date == now.Date.AddDays(-1))
        {
            return "Yesterday";
        }

        return visit.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    private static bool Contains(
        string? value,
        string term)
    {
        return value?.Contains(
            term,
            StringComparison.OrdinalIgnoreCase) == true;
    }

    private static MembershipStatus? GetMemberStatus(
        IReadOnlyCollection<MembershipStatusData> memberships,
        DateOnly asOfDate)
    {
        if (memberships.Count == 0)
        {
            return null;
        }

        var coveringMembership =
            memberships
                .Where(x =>
                    x.StartDate <= asOfDate &&
                    asOfDate < x.EndDate)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

        if (coveringMembership is not null)
        {
            if (coveringMembership.Status == MembershipStatus.Cancelled)
            {
                return MembershipStatus.Expired;
            }

            if (coveringMembership.IsFrozen)
            {
                return MembershipStatus.Frozen;
            }

            return MembershipStatus.Active;
        }

        var futureMembership =
            memberships
                .Where(x =>
                    x.Status != MembershipStatus.Cancelled &&
                    x.StartDate > asOfDate)
                .OrderBy(x => x.StartDate)
                .FirstOrDefault();

        if (futureMembership is not null)
        {
            return null;
        }

        return MembershipStatus.Expired;
    }

    private sealed record MembershipStatusData(
        int MembershipId,
        DateOnly StartDate,
        DateOnly EndDate,
        MembershipStatus Status,
        bool IsFrozen);
}