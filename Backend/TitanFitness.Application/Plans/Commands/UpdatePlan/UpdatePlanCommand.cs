using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Commands.UpdatePlan;

public sealed record UpdatePlanCommand(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope,
    bool IsPublished,
    IReadOnlyCollection<int>? BranchIds);