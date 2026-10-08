using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Memberships.Commands.FreezeMembership;

public sealed record FreezeMembershipCommand(
    int MembershipId,
    DateOnly StartDate,
    int DurationInMonths,
    FreezeReason Reason,
    string? AdditionalNotes);

public sealed record FreezeMembershipResult(
    int FreezeId,
    int MembershipId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DurationInMonths,
    DateOnly NewMembershipEndDate);