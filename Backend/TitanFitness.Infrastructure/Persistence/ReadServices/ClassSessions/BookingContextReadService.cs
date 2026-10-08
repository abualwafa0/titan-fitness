using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.ClassSessions.Queries.GetBookingContext;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.ClassSessions;

public sealed class BookingContextReadService
    : IBookingContextReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public BookingContextReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext =
            dbContext;
    }

    public async Task<BookingContextDto?> GetAsync(
        int sessionId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var session =
            await _dbContext.ClassSessions
                .AsNoTracking()
                .Include(x =>
                    x.Bookings)
                .FirstOrDefaultAsync(
                    x =>
                        x.SessionId ==
                        sessionId,
                    cancellationToken);

        if (session is null)
        {
            return null;
        }

        var branchName =
            await _dbContext.Branches
                .AsNoTracking()
                .Where(x =>
                    x.BranchId ==
                    session.BranchId)
                .Select(x =>
                    x.BranchName)
                .FirstAsync(
                    cancellationToken);

        string? studioName =
            null;

        if (session.StudioId.HasValue)
        {
            studioName =
                await _dbContext.Studios
                    .AsNoTracking()
                    .Where(x =>
                        x.StudioId ==
                        session.StudioId.Value)
                    .Select(x =>
                        x.StudioName)
                    .FirstOrDefaultAsync(
                        cancellationToken);
        }



        string? trainerName =
            null;

        if (session.TrainerId.HasValue)
        {
            trainerName =
                await _dbContext.Trainers
                    .AsNoTracking()
                    .Where(x =>
                        x.TrainerId ==
                        session.TrainerId.Value)
                    .Select(x =>
                        x.TrainerName)
                    .FirstOrDefaultAsync(
                        cancellationToken);
        }

        var bookedPlaces =
            session.Bookings.Count(x =>
                x.Status ==
                    BookingStatus.Booked ||
                x.Status ==
                    BookingStatus.Attended ||
                x.Status ==
                    BookingStatus.NoShow);

        var waitlistCount =
            session.Bookings.Count(x =>
                x.Status ==
                BookingStatus.Waitlisted);

        return new BookingContextDto(
            session.SessionId,
            session.ClassName,
            session.BranchId,
            branchName,
            session.StudioId,
            studioName,
            session.TrainerId,
            trainerName,
            session.SessionDate,
            session.StartTime,
            session.DurationInMinutes,
            session.CapacityLimit,
            bookedPlaces,
            Math.Max(
                0,
                session.CapacityLimit -
                bookedPlaces),
            waitlistCount,
            session.GetEffectiveStatus(
                now),
            session.Description);
    }
}