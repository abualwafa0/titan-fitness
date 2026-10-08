using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Plans.Queries.GetPlanDetails;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Plans;

public sealed class PlanDetailsReadService
    : IPlanDetailsReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public PlanDetailsReadService(TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlanDetailsDto?> GetByIdAsync(
        int planId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Plans
            .AsNoTracking()
            .Where(x => x.PlanId == planId)
            .Select(x => new PlanDetailsDto(
                x.PlanId,
                x.PlanName,
                x.Price,
                x.DurationInMonths,
                x.MaximumFreezeDays,
                x.MaximumNumberOfFreezes,
                x.GuestPassQuota,
                x.AccessScope,
                x.IsPublished,
                _dbContext.Memberships.Count(
                    membership =>
                        membership.PlanId == x.PlanId)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}