namespace TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

public interface IClassScheduleReadService
{
    Task<ClassScheduleDto> GetAsync(
        DateOnly date,
        DateOnly? toDate,
        DateTime now,
        int? branchId,
        CancellationToken cancellationToken = default);
}