using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

public sealed class GetStudiosByBranchQueryHandler
{
    private readonly IStudiosByBranchReadService _readService;
    private readonly IBranchRepository _branchRepository;

    public GetStudiosByBranchQueryHandler(
        IStudiosByBranchReadService readService,
        IBranchRepository branchRepository)
    {
        _readService = readService;
        _branchRepository = branchRepository;
    }

    public async Task<IReadOnlyCollection<StudioDto>> Handle(
        GetStudiosByBranchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.BranchId <= 0)
        {
            throw new ArgumentException(
                "Branch id must be greater than zero.");
        }

        var branchExists = await _branchRepository.ExistsAsync(
            query.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            throw new NotFoundException(
                $"Branch with id {query.BranchId} was not found.");
        }

        return await _readService.GetAsync(
            query.BranchId,
            cancellationToken);
    }
}