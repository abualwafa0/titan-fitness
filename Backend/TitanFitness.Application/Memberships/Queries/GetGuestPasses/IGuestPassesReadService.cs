namespace TitanFitness.Application.Memberships.Queries.GetGuestPasses;

public interface IGuestPassesReadService
{
    Task<GuestPassesDto?> GetAsync(
        int membershipId,
        CancellationToken cancellationToken = default);
}