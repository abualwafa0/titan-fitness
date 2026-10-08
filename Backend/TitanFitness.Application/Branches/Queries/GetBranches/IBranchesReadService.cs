namespace TitanFitness.Application.Branches.Queries.GetBranches;

public interface IBranchesReadService
{
    Task<IReadOnlyCollection<BranchDto>> GetAsync(
        CancellationToken cancellationToken = default);
}