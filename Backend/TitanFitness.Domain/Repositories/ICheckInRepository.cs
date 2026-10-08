using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface ICheckInRepository
{
    Task<CheckIn?> GetByIdAsync(
        int checkInId,
        CancellationToken cancellationToken = default);

    Task<CheckIn?> GetOpenAdmittedCheckInForMemberAsync(
        int memberId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default);
}