namespace TitanFitness.Application.ClassSessions.Commands.ScheduleClass;

public sealed record ScheduleClassCommand(
    string ClassName,
    int BranchId,
    int? StudioId,
    int? TrainerId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationInMinutes,
    int? CapacityLimit,
    string? Description);

public sealed record ScheduleClassResult(
    int SessionId,
    string ClassName,
    int BranchId,
    int? StudioId,
    int? TrainerId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationInMinutes,
    int CapacityLimit);