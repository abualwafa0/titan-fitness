namespace TitanFitness.API.Contracts.Memberships;

public sealed class PurchaseMembershipRequest
{
    public int PlanId { get; set; }

    public DateOnly StartDate { get; set; }
}