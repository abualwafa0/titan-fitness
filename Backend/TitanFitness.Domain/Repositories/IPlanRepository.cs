using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(
        int planId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int planId,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAtBranchAsync(
        int planId,
        int branchId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
    string planName,
    int? excludePlanId,
    CancellationToken cancellationToken = default);

    Task AddAsync(
        Plan plan,
        CancellationToken cancellationToken = default);
}