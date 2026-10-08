using TitanFitness.Application.Common.Models;

namespace TitanFitness.Application.Plans.Queries.GetPlanCatalogue;

public sealed class GetPlanCatalogueQueryHandler
{
    private readonly IPlanCatalogueReadService _readService;

    public GetPlanCatalogueQueryHandler(
        IPlanCatalogueReadService readService)
    {
        _readService = readService;
    }

    public async Task<PagedResult<PlanCatalogueItemDto>> Handle(
        GetPlanCatalogueQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.BranchId.HasValue &&
            query.BranchId.Value <= 0)
        {
            throw new ArgumentException(
                "Branch id must be greater than zero.");
        }

        if (query.PageNumber <= 0)
        {
            throw new ArgumentException(
                "Page number must be greater than zero.");
        }

        if (query.PageSize <= 0)
        {
            throw new ArgumentException(
                "Page size must be greater than zero.");
        }

        if (query.MinPrice.HasValue &&
            query.MinPrice.Value < 0)
        {
            throw new ArgumentException(
                "Minimum price cannot be negative.");
        }

        if (query.MinPrice.HasValue &&
            query.MaxPrice.HasValue &&
            query.MinPrice.Value > query.MaxPrice.Value)
        {
            throw new ArgumentException(
                "Minimum price cannot be greater than maximum price.");
        }

        return await _readService.GetAsync(
            query.Search,
            query.BranchId,
            query.IsPublished,
            query.Durations,
            query.AccessScope,
            query.MinPrice,
            query.MaxPrice,
            query.SortBy,
            query.SortDirection,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PlanFilterOptionsDto> GetFilterOptions(
        CancellationToken cancellationToken = default)
    {
        return await _readService.GetFilterOptionsAsync(
            cancellationToken);
    }
}