using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Queries.GetPlanDetails;

public sealed record PlanDetailsDto(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope,
    bool IsPublished,
    int SoldMembershipsCount);