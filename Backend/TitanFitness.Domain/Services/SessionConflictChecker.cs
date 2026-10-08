using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Domain.Services;

public sealed class SessionConflictChecker
{
    private readonly IClassSessionRepository _classSessionRepository;

    public SessionConflictChecker(
        IClassSessionRepository classSessionRepository)
    {
        _classSessionRepository = classSessionRepository
            ?? throw new ArgumentNullException(
                nameof(classSessionRepository));
    }

    public async Task EnsureNoConflictsAsync(
        int? trainerId,
        int? studioId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        if (trainerId.HasValue && trainerId.Value <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(trainerId));

        if (studioId.HasValue && studioId.Value <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(studioId));

        ArgumentNullException.ThrowIfNull(timeRange);

        if (trainerId.HasValue)
        {
            var trainerConflict =
                await _classSessionRepository
                    .HasTrainerConflictAsync(
                        trainerId.Value,
                        timeRange,
                        excludeSessionId,
                        cancellationToken);

            if (trainerConflict)
            {
                throw new InvalidOperationException(
                    "The trainer already has an overlapping class session.");
            }
        }

        if (studioId.HasValue)
        {
            var studioConflict =
                await _classSessionRepository
                    .HasStudioConflictAsync(
                        studioId.Value,
                        timeRange,
                        excludeSessionId,
                        cancellationToken);

            if (studioConflict)
            {
                throw new InvalidOperationException(
                    "The studio already has an overlapping class session.");
            }
        }
    }
}