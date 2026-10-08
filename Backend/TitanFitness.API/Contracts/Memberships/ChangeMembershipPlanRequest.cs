using TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;

namespace TitanFitness.API.Contracts.Memberships;

public sealed class ChangeMembershipPlanRequest
{
    public int NewPlanId { get; set; }

    public PlanChangeTiming Timing { get; set; }
}