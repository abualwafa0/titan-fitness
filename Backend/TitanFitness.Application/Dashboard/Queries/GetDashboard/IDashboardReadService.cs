namespace TitanFitness.Application.Dashboard.Queries.GetDashboard;

public interface IDashboardReadService
{
    Task<DashboardDto> GetAsync(
        DateOnly date,
        DateTime now,
        int? branchId,
        CancellationToken cancellationToken = default);
}