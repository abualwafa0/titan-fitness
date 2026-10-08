using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler
{
    private readonly IDashboardReadService _readService;
    private readonly IBranchRepository _branchRepository;

    public GetDashboardQueryHandler(
        IDashboardReadService readService,
        IBranchRepository branchRepository)
    {
        _readService = readService;
        _branchRepository = branchRepository;
    }

    public async Task<DashboardDto> Handle(
        GetDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.BranchId.HasValue)
        {
            if (query.BranchId.Value <= 0)
            {
                throw new ArgumentException(
                    "Branch id must be greater than zero.",
                    nameof(query.BranchId));
            }

            var branchExists =
                await _branchRepository.ExistsAsync(
                    query.BranchId.Value,
                    cancellationToken);

            if (!branchExists)
            {
                throw new NotFoundException(
                    $"Branch with id {query.BranchId.Value} was not found.");
            }
        }

        var now = DateTime.Now;
        var date = DateOnly.FromDateTime(now);

        return await _readService.GetAsync(
            date,
            now,
            query.BranchId,
            cancellationToken);
    }
}