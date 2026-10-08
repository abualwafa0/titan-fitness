using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Queries.GetPlanCatalogue;

public sealed record PlanCatalogueItemDto(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope,
    bool IsPublished);