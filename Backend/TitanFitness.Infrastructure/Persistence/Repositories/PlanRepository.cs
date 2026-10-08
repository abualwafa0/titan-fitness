using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class PlanRepository
    : IPlanRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public PlanRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Plan?> GetByIdAsync(
        int planId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Plans
            .Include(x => x.AvailableBranches)
            .FirstOrDefaultAsync(
                x => x.PlanId == planId,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(
        int planId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Plans
            .AnyAsync(
                x => x.PlanId == planId,
                cancellationToken);
    }

    public Task<bool> IsAvailableAtBranchAsync(
        int planId,
        int branchId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Plans
            .Where(x => x.PlanId == planId)
            .AnyAsync(
                x =>
                    !x.AvailableBranches.Any() ||
                    x.AvailableBranches.Any(
                        branch =>
                            branch.BranchId ==
                            branchId),
                cancellationToken);
    }
    public Task<bool> NameExistsAsync(
    string planName,
    int? excludePlanId,
    CancellationToken cancellationToken = default)
    {
        var normalized = planName.Trim().ToLower();

        return _dbContext.Plans
            .AnyAsync(
                x => x.PlanName.ToLower() == normalized &&
                     (!excludePlanId.HasValue ||
                      x.PlanId != excludePlanId.Value),
                cancellationToken);
    }
    public async Task AddAsync(
        Plan plan,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Plans.AddAsync(
            plan,
            cancellationToken);
    }
}