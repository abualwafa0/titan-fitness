using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Memberships.Queries.GetFreezeContext;

public sealed record FreezeContextDto(
    int MembershipId,
    int MemberId,
    string MemberName,
    string MembershipNumber,
    int PlanId,
    string PlanName,
    MembershipStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int FreezeDaysUsed,
    int NumberOfFreezesUsed,
    int RemainingFreezeDays,
    int RemainingNumberOfFreezes,
    IReadOnlyCollection<int> AllowedDurationsInMonths);