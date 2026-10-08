using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Plans.Commands.CreatePlan;

public sealed record CreatePlanCommand(
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope,
    bool IsPublished,
    IReadOnlyCollection<int>? BranchIds);

public sealed record CreatePlanResult(
    int PlanId);