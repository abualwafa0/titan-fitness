namespace TitanFitness.Application.Branches.Queries.GetBranches;

public sealed class GetBranchesQueryHandler
{
    private readonly IBranchesReadService _readService;

    public GetBranchesQueryHandler(
        IBranchesReadService readService)
    {
        _readService = readService;
    }

    public async Task<IReadOnlyCollection<BranchDto>> Handle(
        GetBranchesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _readService.GetAsync(
            cancellationToken);
    }
}