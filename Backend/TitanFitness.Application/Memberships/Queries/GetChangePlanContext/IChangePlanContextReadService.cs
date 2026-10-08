namespace TitanFitness.Application.Memberships.Queries.GetChangePlanContext;

public interface IChangePlanContextReadService
{
    Task<ChangePlanContextDto?> GetAsync(
        int membershipId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);
}