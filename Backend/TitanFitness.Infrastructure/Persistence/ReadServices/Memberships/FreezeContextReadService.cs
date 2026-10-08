using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Memberships.Queries.GetFreezeContext;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Memberships;

public sealed class FreezeContextReadService
    : IFreezeContextReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public FreezeContextReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FreezeContextDto?> GetAsync(
        int membershipId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.Memberships
            .AsNoTracking()
            .Include(x => x.Freezes)
            .FirstOrDefaultAsync(
                x => x.MembershipId == membershipId,
                cancellationToken);

        if (membership is null)
        {
            return null;
        }

        var member = await _dbContext.Members
            .AsNoTracking()
            .FirstAsync(
                x => x.MemberId ==
                     membership.MemberId,
                cancellationToken);

        var planName = await _dbContext.Plans
            .AsNoTracking()
            .Where(x =>
                x.PlanId == membership.PlanId)
            .Select(x => x.PlanName)
            .FirstAsync(cancellationToken);

        return new FreezeContextDto(
            membership.MembershipId,
            member.MemberId,
            member.FullName,
            member.MembershipNumber,
            membership.PlanId,
            planName,
            membership.GetEffectiveStatus(asOfDate),
            membership.StartDate,
            membership.EndDate,
            membership.AgreedTerms.MaximumFreezeDays,
            membership.AgreedTerms
                .MaximumNumberOfFreezes,
            membership.GetTotalFrozenDays(),
            membership.Freezes.Count,
            membership.GetRemainingFreezeDays(),
            membership.GetRemainingNumberOfFreezes(),
            new[] { 1, 2, 3 });
    }
}