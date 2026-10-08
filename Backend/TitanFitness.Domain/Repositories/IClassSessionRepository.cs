using TitanFitness.Domain.Entities;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Domain.Repositories;

public interface IClassSessionRepository
{
    Task<ClassSession?> GetByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<bool> HasTrainerConflictAsync(
        int trainerId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasStudioConflictAsync(
        int studioId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasMemberBookingConflictAsync(
        int memberId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassSession>> GetFutureSessionsByTrainerAsync(
        int trainerId,
        DateTime fromDateTime,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ClassSession classSession,
        CancellationToken cancellationToken = default);
}