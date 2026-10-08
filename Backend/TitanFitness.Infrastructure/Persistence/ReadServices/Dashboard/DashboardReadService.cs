using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Dashboard.Queries.GetDashboard;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Dashboard;

public sealed class DashboardReadService
    : IDashboardReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public DashboardReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardDto> GetAsync(
        DateOnly date,
        DateTime now,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        var dayStart =
            date.ToDateTime(
                TimeOnly.MinValue);

        var dayEnd =
            dayStart.AddDays(1);

        var previousWeekDate =
            date.AddDays(-7);

        var previousWeekStart =
            previousWeekDate.ToDateTime(
                TimeOnly.MinValue);

        var previousWeekEnd =
            previousWeekStart.AddDays(1);

        var checkInsTodayQuery =
            _dbContext.CheckIns
                .AsNoTracking()
                .Where(x =>
                    x.Result ==
                        CheckInResult.Admitted &&
                    x.CheckInDateTime >= dayStart &&
                    x.CheckInDateTime < dayEnd);

        var previousCheckInsQuery =
            _dbContext.CheckIns
                .AsNoTracking()
                .Where(x =>
                    x.Result ==
                        CheckInResult.Admitted &&
                    x.CheckInDateTime >= previousWeekStart &&
                    x.CheckInDateTime < previousWeekEnd);

        if (branchId.HasValue)
        {
            checkInsTodayQuery =
                checkInsTodayQuery.Where(x =>
                    x.BranchId ==
                    branchId.Value);

            previousCheckInsQuery =
                previousCheckInsQuery.Where(x =>
                    x.BranchId ==
                    branchId.Value);
        }

        var checkInsToday =
            await checkInsTodayQuery
                .CountAsync(
                    cancellationToken);

        var checkInsSameWeekdayLastWeek =
            await previousCheckInsQuery
                .CountAsync(
                    cancellationToken);

        var memberships =
            await _dbContext.Memberships
                .AsNoTracking()
                .Include(x =>
                    x.Freezes)
                .Where(x =>
                    x.Status !=
                        MembershipStatus.Cancelled &&
                    x.StartDate <= date &&
                    date < x.EndDate)
                .ToListAsync(
                    cancellationToken);

        if (branchId.HasValue)
        {
            var branchMemberIds =
                await _dbContext.Members
                    .AsNoTracking()
                    .Where(x =>
                        x.HomeBranchId ==
                        branchId.Value)
                    .Select(x =>
                        x.MemberId)
                    .ToListAsync(
                        cancellationToken);

            var branchMemberSet =
                branchMemberIds
                    .ToHashSet();

            memberships =
                memberships
                    .Where(x =>
                        branchMemberSet.Contains(
                            x.MemberId))
                    .ToList();
        }

        var activeMemberIds =
            memberships
                .Where(x =>
                    x.GetEffectiveStatus(date) ==
                    MembershipStatus.Active)
                .Select(x =>
                    x.MemberId)
                .Distinct()
                .ToHashSet();

        var activeMembers =
            activeMemberIds.Count;

        var insideQuery =
            _dbContext.CheckIns
                .AsNoTracking()
                .Where(x =>
                    x.Result ==
                        CheckInResult.Admitted &&
                    x.CheckOutDateTime ==
                        null &&
                    x.CheckInDateTime >= dayStart &&
                    x.CheckInDateTime < dayEnd);

        if (branchId.HasValue)
        {
            insideQuery =
                insideQuery.Where(x =>
                    x.BranchId ==
                    branchId.Value);
        }

        var insideMemberIds =
            await insideQuery
                .Select(x =>
                    x.MemberId)
                .Distinct()
                .ToListAsync(
                    cancellationToken);

        var membersCurrentlyInside =
            insideMemberIds.Count(
                memberId =>
                    activeMemberIds.Contains(
                        memberId));

        var todaySessionsQuery =
            _dbContext.ClassSessions
                .AsNoTracking()
                .Include(x =>
                    x.Bookings)
                .Where(x =>
                    x.SessionDate == date &&
                    x.Status !=
                        SessionStatus.Cancelled);

        if (branchId.HasValue)
        {
            todaySessionsQuery =
                todaySessionsQuery.Where(x =>
                    x.BranchId ==
                    branchId.Value);
        }

        var todaySessions =
            await todaySessionsQuery
                .ToListAsync(
                    cancellationToken);

        var upcomingSessionEntities =
            todaySessions
                .Where(x =>
                    x.GetEffectiveStatus(now) is
                        SessionStatus.Open or
                        SessionStatus.InProgress)
                .OrderBy(x =>
                    x.StartTime)
                .ToList();

        var branchIds =
            todaySessions
                .Select(x =>
                    x.BranchId)
                .Distinct()
                .ToList();

        var studioIds =
            todaySessions
                .Where(x =>
                    x.StudioId.HasValue)
                .Select(x =>
                    x.StudioId!.Value)
                .Distinct()
                .ToList();

        var trainerIds =
            todaySessions
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

        DashboardSessionDto MapSession(
            ClassSession session)
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

            return new DashboardSessionDto(
                session.SessionId,
                session.ClassName,
                session.BranchId,
                branchName ?? "Unknown Branch",
                session.StudioId,
                studioName,
                session.TrainerId,
                trainerName,
                session.SessionDate,
                session.StartTime,
                session.DurationInMinutes,
                bookedPlaces,
                session.CapacityLimit,
                waitlistCount,
                session.GetEffectiveStatus(
                    now));
        }

        var currentRunningSession =
            todaySessions
                .Where(x =>
                    x.GetEffectiveStatus(now) ==
                    SessionStatus.InProgress)
                .OrderBy(x =>
                    x.StartTime)
                .Select(
                    MapSession)
                .FirstOrDefault();

        var upcomingSessions =
            upcomingSessionEntities
                .Select(
                    MapSession)
                .ToList();

        var bookingsToday =
            todaySessions
                .SelectMany(x =>
                    x.Bookings)
                .Count(x =>
                    x.Status !=
                        BookingStatus.Cancelled &&
                    x.Status !=
                        BookingStatus.Waitlisted);

        var totalCapacity =
            todaySessions.Sum(x =>
                x.CapacityLimit);

        var totalBooked =
            todaySessions.Sum(session =>
                session.Bookings.Count(x =>
                    x.Status ==
                        BookingStatus.Booked ||
                    x.Status ==
                        BookingStatus.Attended ||
                    x.Status ==
                        BookingStatus.NoShow));

        var capacityFilledPercentage =
            totalCapacity == 0
                ? 0m
                : Math.Round(
                    (decimal)totalBooked /
                    totalCapacity *
                    100m,
                    2);

        return new DashboardDto(
            date,
            branchId,
            checkInsToday,
            checkInsSameWeekdayLastWeek,
            activeMembers,
            membersCurrentlyInside,
            upcomingSessions,
            currentRunningSession,
            bookingsToday,
            capacityFilledPercentage);
    }
}