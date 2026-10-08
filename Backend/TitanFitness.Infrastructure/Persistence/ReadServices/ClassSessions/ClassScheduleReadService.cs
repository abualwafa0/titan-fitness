using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.ClassSessions;

public sealed class ClassScheduleReadService
    : IClassScheduleReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public ClassScheduleReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext =
            dbContext;
    }

    public async Task<ClassScheduleDto> GetAsync(
        DateOnly date,
        DateOnly? toDate,
        DateTime now,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        var endDate = toDate ?? date;

        var query =
            _dbContext.ClassSessions
                .AsNoTracking()
                .Include(x =>
                    x.Bookings)
                .Where(x =>
                    x.SessionDate >= date &&
                    x.SessionDate <= endDate);

        if (branchId.HasValue)
        {
            query =
                query.Where(x =>
                    x.BranchId ==
                    branchId.Value);
        }

        var sessions =
            await query
                .OrderBy(x =>
                    x.SessionDate)
                .ThenBy(x =>
                    x.StartTime)
                .ToListAsync(
                    cancellationToken);

        var branchIds =
            sessions
                .Select(x =>
                    x.BranchId)
                .Distinct()
                .ToList();

        var studioIds =
            sessions
                .Where(x =>
                    x.StudioId.HasValue)
                .Select(x =>
                    x.StudioId!.Value)
                .Distinct()
                .ToList();

        var trainerIds =
            sessions
                .Where(x =>
                    x.TrainerId.HasValue)
                .Select(x =>
                    x.TrainerId!.Value)
                .Distinct()
                .ToList();

        var branches =
            await _dbContext.Branches
                .AsNoTracking()
                .Where(x =>
                    branchIds.Contains(
                        x.BranchId))
                .ToDictionaryAsync(
                    x => x.BranchId,
                    x => x.BranchName,
                    cancellationToken);

        var studios =
            await _dbContext.Studios
                .AsNoTracking()
                .Where(x =>
                    studioIds.Contains(
                        x.StudioId))
                .ToDictionaryAsync(
                    x => x.StudioId,
                    x => x.StudioName,
                    cancellationToken);

        var trainers =
            await _dbContext.Trainers
                .AsNoTracking()
                .Where(x =>
                    trainerIds.Contains(
                        x.TrainerId))
                .ToDictionaryAsync(
                    x => x.TrainerId,
                    x => x.TrainerName,
                    cancellationToken);

        var items =
            sessions
                .Select(session =>
                {
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

                    string? trainerName =
                        null;

                    if (session.TrainerId.HasValue)
                    {
                        trainers.TryGetValue(
                            session.TrainerId.Value,
                            out trainerName);
                    }

                    string? studioName =
                        null;

                    if (session.StudioId.HasValue)
                    {
                        studios.TryGetValue(
                            session.StudioId.Value,
                            out studioName);
                    }

                    branches.TryGetValue(
                        session.BranchId,
                        out var branchName);

                    return new ClassScheduleItemDto(
                        session.SessionId,
                        session.ClassName,
                        session.BranchId,
                        branchName ??
                            "Unknown Branch",
                        session.StudioId,
                        studioName,
                        session.TrainerId,
                        trainerName,
                        session.SessionDate,
                        session.StartTime,
                        session.DurationInMinutes,
                        session.CapacityLimit,
                        bookedPlaces,
                        waitlistCount,
                        session.GetEffectiveStatus(
                            now));
                })
                .ToList();

        var totalBookedPlaces =
            items.Sum(x =>
                x.BookedPlaces);

        var totalCapacity =
            items.Sum(x =>
                x.CapacityLimit);

        decimal capacityFilledPercentage;

        if (items.Count == 0)
        {
            capacityFilledPercentage =
                0m;
        }
        else
        {
            capacityFilledPercentage =
                Math.Round(
                    items.Average(x =>
                        x.CapacityLimit <= 0
                            ? 0m
                            : (decimal)x.BookedPlaces /
                              x.CapacityLimit *
                              100m),
                    2);
        }

        return new ClassScheduleDto(
            date,
            toDate,
            branchId,
            totalBookedPlaces,
            totalCapacity,
            capacityFilledPercentage,
            items);
    }
}