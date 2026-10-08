using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Members.Queries.GetMemberProfile;

public sealed record MemberProfileDto(
    int MemberId,
    string MembershipNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    DateOnly JoinedDate,
    string? Photo,
    int HomeBranchId,
    string HomeBranchName,
    MembershipStatus? Status,
    MemberCurrentMembershipDto? CurrentMembership,
    IReadOnlyCollection<MemberMembershipHistoryDto> Memberships,
    IReadOnlyCollection<MemberRecentActivityDto> RecentActivities);

public sealed record MemberCurrentMembershipDto(
    int MembershipId,
    int PlanId,
    string PlanName,
    decimal PricePaid,
    DateOnly StartDate,
    DateOnly EndDate,
    MembershipStatus Status,
    int FreezesUsed,
    int MaximumNumberOfFreezes,
    int FreezeDaysUsed,
    int MaximumFreezeDays,
    int GuestPassesUsed,
    int GuestPassQuota);

public sealed record MemberMembershipHistoryDto(
    int MembershipId,
    int PlanId,
    string PlanName,
    DateTime PurchaseDate,
    DateOnly StartDate,
    DateOnly EndDate,
    MembershipStatus Status,
    decimal PricePaid,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    AccessScope AccessScope);

public sealed record MemberRecentActivityDto(
    string ActivityType,
    string Title,
    string? Details,
    DateTime OccurredOn);