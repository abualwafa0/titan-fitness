using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class ClassSessionRepository
    : IClassSessionRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public ClassSessionRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClassSession?> GetByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ClassSessions
            .Include(x => x.Bookings)
            .FirstOrDefaultAsync(
                x => x.SessionId == sessionId,
                cancellationToken);
    }

    public async Task<bool> HasTrainerConflictAsync(
        int trainerId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        var candidates =
            await GetConflictCandidatesAsync(
                timeRange,
                cancellationToken);

        return candidates.Any(
            session =>
                session.TrainerId == trainerId &&
                (!excludeSessionId.HasValue ||
                 session.SessionId !=
                 excludeSessionId.Value) &&
                session.GetTimeRange()
                    .Overlaps(timeRange));
    }

    public async Task<bool> HasStudioConflictAsync(
        int studioId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        var candidates =
            await GetConflictCandidatesAsync(
                timeRange,
                cancellationToken);

        return candidates.Any(
            session =>
                session.StudioId == studioId &&
                (!excludeSessionId.HasValue ||
                 session.SessionId !=
                 excludeSessionId.Value) &&
                session.GetTimeRange()
                    .Overlaps(timeRange));
    }

    public async Task<bool> HasMemberBookingConflictAsync(
        int memberId,
        TimeRange timeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        var fromDate =
            DateOnly.FromDateTime(
                timeRange.Start.AddDays(-1));

        var toDate =
            DateOnly.FromDateTime(
                timeRange.End);

        var candidates =
            await _dbContext.ClassSessions
                .AsNoTracking()
                .Include(x => x.Bookings)
                .Where(x =>
                    x.Status !=
                        SessionStatus.Cancelled &&
                    x.SessionDate >= fromDate &&
                    x.SessionDate <= toDate &&
                    x.Bookings.Any(
                        booking =>
                            booking.MemberId ==
                                memberId &&
                            booking.Status !=
                                BookingStatus.Cancelled))
                .ToListAsync(
                    cancellationToken);

        return candidates.Any(
            session =>
                (!excludeSessionId.HasValue ||
                 session.SessionId !=
                 excludeSessionId.Value) &&
                session.GetTimeRange()
                    .Overlaps(timeRange));
    }

    public async Task<IReadOnlyList<ClassSession>>
        GetFutureSessionsByTrainerAsync(
            int trainerId,
            DateTime fromDateTime,
            CancellationToken cancellationToken = default)
    {
        var fromDate =
            DateOnly.FromDateTime(
                fromDateTime);

        var sessions =
            await _dbContext.ClassSessions
                .Where(x =>
                    x.TrainerId == trainerId &&
                    x.Status !=
                        SessionStatus.Cancelled &&
                    x.SessionDate >= fromDate)
                .ToListAsync(
                    cancellationToken);

        return sessions
            .Where(x =>
                x.GetStartDateTime() >
                fromDateTime)
            .OrderBy(x =>
                x.GetStartDateTime())
            .ToList();
    }

    public async Task AddAsync(
        ClassSession classSession,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ClassSessions.AddAsync(
            classSession,
            cancellationToken);
    }

    private Task<List<ClassSession>>
        GetConflictCandidatesAsync(
            TimeRange timeRange,
            CancellationToken cancellationToken)
    {
        var fromDate =
            DateOnly.FromDateTime(
                timeRange.Start.AddDays(-1));

        var toDate =
            DateOnly.FromDateTime(
                timeRange.End);

        return _dbContext.ClassSessions
            .AsNoTracking()
            .Where(x =>
                x.Status !=
                    SessionStatus.Cancelled &&
                x.SessionDate >= fromDate &&
                x.SessionDate <= toDate)
            .ToListAsync(
                cancellationToken);
    }
}