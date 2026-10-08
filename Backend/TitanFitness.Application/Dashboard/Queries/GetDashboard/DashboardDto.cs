using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Dashboard.Queries.GetDashboard;

public sealed record DashboardDto(
    DateOnly Date,
    int? BranchId,
    int CheckInsToday,
    int CheckInsSameWeekdayLastWeek,
    int ActiveMembers,
    int MembersCurrentlyInside,
    IReadOnlyCollection<DashboardSessionDto> UpcomingSessions,
    DashboardSessionDto? CurrentRunningSession,
    int BookingsToday,
    decimal CapacityFilledPercentage);

public sealed record DashboardSessionDto(
    int SessionId,
    string ClassName,
    int BranchId,
    string BranchName,
    int? StudioId,
    string? StudioName,
    int? TrainerId,
    string? TrainerName,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationMinutes,
    int BookedPlaces,
    int CapacityLimit,
    int WaitlistCount,
    SessionStatus Status);