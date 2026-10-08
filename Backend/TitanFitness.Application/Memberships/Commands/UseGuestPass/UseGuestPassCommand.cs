namespace TitanFitness.Application.Memberships.Commands.UseGuestPass;

public sealed record UseGuestPassCommand(
    int MembershipId,
    int GuestPassId);

public sealed record UseGuestPassResult(
    int GuestPassId,
    int MembershipId,
    DateOnly UsedOn);