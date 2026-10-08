using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Queries.GetPlanCatalogue;

public sealed record GetPlanCatalogueQuery(
    string? Search,
    int? BranchId,
    bool? IsPublished,
    IReadOnlyCollection<int>? Durations,
    AccessScope? AccessScope,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? SortBy,
    string? SortDirection,
    int PageNumber = 1,
    int PageSize = 10);