using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Queries.SearchMembersLookup;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Members;

public sealed class MemberLookupReadService
    : IMemberLookupReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public MemberLookupReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MemberLookupDto>> SearchAsync(
        string? search,
        DateOnly asOfDate,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Members
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term =
                search.Trim();

            query = query.Where(x =>
                x.FullName.Contains(term) ||
                x.MembershipNumber.Contains(term));
        }

        var members = await query
            .OrderBy(x =>
                x.FullName)
            .Take(limit)
            .Select(member => new
            {
                member.MemberId,
                member.FullName,
                member.MembershipNumber,
                member.Photo,

                Memberships = _dbContext.Memberships
                    .Where(membership =>
                        membership.MemberId ==
                            member.MemberId)
                    .Select(membership => new
                    {
                        membership.StartDate,
                        membership.EndDate,
                        membership.Status,

                        IsFrozen = _dbContext.Freezes.Any(
                            freeze =>
                                freeze.MembershipId ==
                                    membership.MembershipId &&
                                freeze.StartDate <=
                                    asOfDate &&
                                asOfDate <
                                    freeze.EndDate)
                    })
                    .ToList()
            })
            .ToListAsync(
                cancellationToken);

        return members
            .Select(member =>
            {
                var status = GetMemberStatus(
                    member.Memberships
                        .Select(x =>
                            new MembershipStatusData(
                                x.StartDate,
                                x.EndDate,
                                x.Status,
                                x.IsFrozen))
                        .ToList(),
                    asOfDate);

                return new MemberLookupDto(
                    member.MemberId,
                    member.FullName,
                    member.MembershipNumber,
                    member.Photo,
                    status);
            })
            .ToList();
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
                    x.StartDate <=
                        asOfDate &&
                    asOfDate <
                        x.EndDate)
                .OrderByDescending(x =>
                    x.StartDate)
                .FirstOrDefault();

        if (coveringMembership is not null)
        {
            if (coveringMembership.Status ==
                MembershipStatus.Cancelled)
            {
                return MembershipStatus.Expired;
            }

            if (coveringMembership.IsFrozen)
            {
                return MembershipStatus.Frozen;
            }

            return MembershipStatus.Active;
        }

        var hasFutureMembership =
            memberships.Any(x =>
                x.Status !=
                    MembershipStatus.Cancelled &&
                x.StartDate >
                    asOfDate);

        if (hasFutureMembership)
        {
            return MembershipStatus.Pending;
        }

        return MembershipStatus.Expired;
    }

    private sealed record MembershipStatusData(
        DateOnly StartDate,
        DateOnly EndDate,
        MembershipStatus Status,
        bool IsFrozen);
}