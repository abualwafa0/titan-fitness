namespace TitanFitness.Application.Memberships.Commands.PurchaseMembership;

public sealed record PurchaseMembershipCommand(
    int MemberId,
    int PlanId,
    DateOnly StartDate);

public sealed record PurchaseMembershipResult(
    int MembershipId,
    int MemberId,
    int PlanId,
    DateOnly StartDate,
    DateOnly EndDate);