using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

public sealed record ClassScheduleDto(
    DateOnly Date,
    DateOnly? ToDate,
    int? BranchId,
    int TotalBookedPlaces,
    int TotalCapacity,
    decimal CapacityFilledPercentage,
    IReadOnlyCollection<ClassScheduleItemDto> Sessions);

public sealed record ClassScheduleItemDto(
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
    int DurationInMinutes,
    int CapacityLimit,
    int BookedPlaces,
    int WaitlistCount,
    SessionStatus Status);