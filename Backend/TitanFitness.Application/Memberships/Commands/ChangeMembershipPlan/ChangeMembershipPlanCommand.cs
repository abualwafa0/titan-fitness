namespace TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;

public enum PlanChangeTiming
{
    AtRenewal = 1,
    Immediately = 2
}

public sealed record ChangeMembershipPlanCommand(
    int MembershipId,
    int NewPlanId,
    PlanChangeTiming Timing);

public sealed record ChangeMembershipPlanResult(
    int NewMembershipId,
    int MemberId,
    int PlanId,
    DateOnly StartDate,
    DateOnly EndDate);