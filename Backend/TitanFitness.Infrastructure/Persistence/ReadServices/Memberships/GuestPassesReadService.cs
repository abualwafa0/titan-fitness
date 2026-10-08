using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Memberships.Queries.GetGuestPasses;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Memberships;

public sealed class GuestPassesReadService
    : IGuestPassesReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public GuestPassesReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GuestPassesDto?> GetAsync(
        int membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.Memberships
            .AsNoTracking()
            .Include(x => x.GuestPasses)
            .FirstOrDefaultAsync(
                x => x.MembershipId == membershipId,
                cancellationToken);

        if (membership is null)
        {
            return null;
        }

        var passes = membership.GuestPasses
            .OrderByDescending(x => x.IssuedOn)
            .ThenByDescending(x => x.GuestPassId)
            .Select(x =>
                new GuestPassDto(
                    x.GuestPassId,
                    x.IssuedOn,
                    x.UsedOn,
                    x.GuestName,
                    x.UsedOn.HasValue))
            .ToList();

        var issuedCount =
            membership.GuestPasses.Count;

        var quota =
            membership.AgreedTerms.GuestPassQuota;

        return new GuestPassesDto(
            membership.MembershipId,
            quota,
            issuedCount,
            Math.Max(0, quota - issuedCount),
            passes);
    }
}