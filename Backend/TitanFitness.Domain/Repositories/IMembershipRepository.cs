using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface IMembershipRepository
{
    Task<Membership?> GetByIdAsync(
        int membershipId,
        CancellationToken cancellationToken = default);

    Task<Membership?> GetCurrentForMemberAsync(
        int memberId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<Membership?> GetLatestForMemberAsync(
        int memberId,
        CancellationToken cancellationToken = default);

    Task<Membership?> GetMostRelevantForMemberAsync(
        int memberId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingMembershipAsync(
        int memberId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludeMembershipId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Membership membership,
        CancellationToken cancellationToken = default);
}