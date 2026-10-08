using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(
        int branchId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int branchId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetAllIdsAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Branch branch,
        CancellationToken cancellationToken = default);
}