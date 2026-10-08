namespace TitanFitness.Application.ClassSessions.Commands.UpdateClass;

public sealed record UpdateClassCommand(
    int SessionId,
    string ClassName,
    int BranchId,
    int? StudioId,
    int? TrainerId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationInMinutes,
    int? CapacityLimit,
    string? Description);