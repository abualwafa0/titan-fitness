namespace TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

public sealed record GetClassScheduleQuery(
    DateOnly Date,
    int? BranchId = null,
    DateOnly? ToDate = null);