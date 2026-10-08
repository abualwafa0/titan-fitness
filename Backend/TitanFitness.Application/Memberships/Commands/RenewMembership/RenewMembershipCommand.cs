namespace TitanFitness.Application.Memberships.Commands.RenewMembership;

public sealed record RenewMembershipCommand(
    int MembershipId);

public sealed record RenewMembershipResult(
    int MembershipId,
    int MemberId,
    int PlanId,
    DateOnly StartDate,
    DateOnly EndDate);