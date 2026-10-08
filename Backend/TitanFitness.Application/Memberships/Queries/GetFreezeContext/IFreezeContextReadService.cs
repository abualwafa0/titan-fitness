namespace TitanFitness.Application.Memberships.Queries.GetFreezeContext;

public interface IFreezeContextReadService
{
    Task<FreezeContextDto?> GetAsync(
        int membershipId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);
}