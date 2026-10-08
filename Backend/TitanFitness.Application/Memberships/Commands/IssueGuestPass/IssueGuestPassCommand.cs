namespace TitanFitness.Application.Memberships.Commands.IssueGuestPass;

public sealed record IssueGuestPassCommand(
    int MembershipId,
    string? GuestName);

public sealed record IssueGuestPassResult(
    int GuestPassId,
    int MembershipId,
    DateOnly IssuedOn,
    string? GuestName,
    int RemainingGuestPasses);