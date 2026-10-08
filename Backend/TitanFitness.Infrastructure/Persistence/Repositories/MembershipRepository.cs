using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class MembershipRepository
    : IMembershipRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public MembershipRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Membership?> GetByIdAsync(
        int membershipId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Memberships
            .Include(x => x.Freezes)
            .Include(x => x.GuestPasses)
            .FirstOrDefaultAsync(
                x => x.MembershipId == membershipId,
                cancellationToken);
    }

    public Task<Membership?> GetCurrentForMemberAsync(
        int memberId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Memberships
            .Include(x => x.Freezes)
            .Include(x => x.GuestPasses)
            .Where(x =>
                x.MemberId == memberId &&
                x.Status != MembershipStatus.Cancelled &&
                x.StartDate <= date &&
                date < x.EndDate)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public Task<Membership?> GetLatestForMemberAsync(
        int memberId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Memberships
            .Include(x => x.Freezes)
            .Include(x => x.GuestPasses)
            .Where(x =>
                x.MemberId == memberId &&
                x.Status != MembershipStatus.Cancelled)
            .OrderByDescending(x => x.EndDate)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<Membership?> GetMostRelevantForMemberAsync(
        int memberId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var currentMembership =
            await _dbContext.Memberships
                .Include(x => x.Freezes)
                .Include(x => x.GuestPasses)
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Status != MembershipStatus.Cancelled &&
                    x.StartDate <= date &&
                    date < x.EndDate)
                .OrderByDescending(x =>
                    x.StartDate)
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (currentMembership is not null)
        {
            return currentMembership;
        }

        var cancelledCoveringMembership =
            await _dbContext.Memberships
                .Include(x => x.Freezes)
                .Include(x => x.GuestPasses)
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Status == MembershipStatus.Cancelled &&
                    x.StartDate <= date &&
                    date < x.EndDate)
                .OrderByDescending(x =>
                    x.StartDate)
                .FirstOrDefaultAsync(
                    cancellationToken);

        var futureMembership =
            await _dbContext.Memberships
                .Include(x => x.Freezes)
                .Include(x => x.GuestPasses)
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Status != MembershipStatus.Cancelled &&
                    x.StartDate > date)
                .OrderBy(x =>
                    x.StartDate)
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (futureMembership is not null)
        {
            return futureMembership;
        }

        if (cancelledCoveringMembership is not null)
        {
            return cancelledCoveringMembership;
        }

        return await _dbContext.Memberships
            .Include(x => x.Freezes)
            .Include(x => x.GuestPasses)
            .Where(x =>
                x.MemberId == memberId &&
                x.StartDate <= date)
            .OrderByDescending(x =>
                x.EndDate)
            .ThenByDescending(x =>
                x.StartDate)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public Task<bool> HasOverlappingMembershipAsync(
        int memberId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludeMembershipId = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Memberships
                .AsQueryable();

        query = query.Where(
            x =>
                x.MemberId == memberId &&
                x.Status != MembershipStatus.Cancelled &&
                x.StartDate < endDate &&
                startDate < x.EndDate);

        if (excludeMembershipId.HasValue)
        {
            query = query.Where(
                x =>
                    x.MembershipId !=
                    excludeMembershipId.Value);
        }

        return query.AnyAsync(
            cancellationToken);
    }

    public async Task AddAsync(
        Membership membership,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Memberships.AddAsync(
            membership,
            cancellationToken);
    }
}