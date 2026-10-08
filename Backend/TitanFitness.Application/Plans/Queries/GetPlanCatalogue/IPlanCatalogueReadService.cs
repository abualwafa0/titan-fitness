using TitanFitness.Application.Common.Models;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Queries.GetPlanCatalogue;

public interface IPlanCatalogueReadService
{
    Task<PagedResult<PlanCatalogueItemDto>> GetAsync(
        string? search,
        int? branchId,
        bool? isPublished,
        IReadOnlyCollection<int>? durations,
        AccessScope? accessScope,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PlanFilterOptionsDto> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default);
}