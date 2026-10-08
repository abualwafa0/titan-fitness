using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Memberships.Queries.GetChangePlanContext;

public sealed record ChangePlanContextDto(
    int MembershipId,
    int MemberId,
    string MemberName,
    string MembershipNumber,
    int CurrentPlanId,
    string CurrentPlanName,
    decimal CurrentPricePaid,
    int CurrentDurationMonths,
    int CurrentMaximumFreezeDays,
    int CurrentMaximumNumberOfFreezes,
    int CurrentGuestPassQuota,
    AccessScope CurrentAccessScope,
    DateOnly CurrentStartDate,
    DateOnly CurrentEndDate,
    MembershipStatus CurrentStatus,
    DateOnly AtRenewalStartDate,
    DateOnly ImmediateStartDate,
    IReadOnlyCollection<ChangePlanOptionDto> AvailablePlans);

public sealed record ChangePlanOptionDto(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope,
    DateOnly AtRenewalEndDate,
    DateOnly ImmediateEndDate);