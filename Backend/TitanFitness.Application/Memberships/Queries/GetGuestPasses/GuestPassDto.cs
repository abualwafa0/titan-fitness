namespace TitanFitness.Application.Memberships.Queries.GetGuestPasses;

public sealed record GuestPassesDto(
    int MembershipId,
    int GuestPassQuota,
    int IssuedCount,
    int RemainingCount,
    IReadOnlyCollection<GuestPassDto> GuestPasses);

public sealed record GuestPassDto(
    int GuestPassId,
    DateOnly IssuedOn,
    DateOnly? UsedOn,
    string? GuestName,
    bool IsUsed);