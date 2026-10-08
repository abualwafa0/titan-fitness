namespace TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

public interface IStudiosByBranchReadService
{
    Task<IReadOnlyCollection<StudioDto>> GetAsync(
        int branchId,
        CancellationToken cancellationToken = default);
}