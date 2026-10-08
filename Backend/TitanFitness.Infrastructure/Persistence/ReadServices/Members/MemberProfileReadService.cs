using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Queries.GetMemberProfile;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Members;

public sealed class MemberProfileReadService
    : IMemberProfileReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public MemberProfileReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MemberProfileDto?> GetAsync(
        int memberId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MemberId == memberId,
                cancellationToken);

        if (member is null)
        {
            return null;
        }

        var branchName =
            await _dbContext.Branches
                .AsNoTracking()
                .Where(x =>
                    x.BranchId ==
                    member.HomeBranchId)
                .Select(x =>
                    x.BranchName)
                .FirstAsync(
                    cancellationToken);

        var memberships =
            await _dbContext.Memberships
                .AsNoTracking()
                .Include(x =>
                    x.Freezes)
                .Include(x =>
                    x.GuestPasses)
                .Where(x =>
                    x.MemberId ==
                    memberId)
                .OrderByDescending(x =>
                    x.StartDate)
                .ThenByDescending(x =>
                    x.PurchaseDate)
                .ToListAsync(
                    cancellationToken);

        var planIds =
            memberships
                .Select(x =>
                    x.PlanId)
                .Distinct()
                .ToList();

        var planNames =
            await _dbContext.Plans
                .AsNoTracking()
                .Where(x =>
                    planIds.Contains(
                        x.PlanId))
                .ToDictionaryAsync(
                    x => x.PlanId,
                    x => x.PlanName,
                    cancellationToken);

        var memberStatus =
            GetMemberStatus(
                memberships,
                asOfDate);

        var selectedMembership =
            memberships
                .Where(x =>
                    x.StartDate <=
                        asOfDate &&
                    asOfDate <
                        x.EndDate)
                .OrderByDescending(x =>
                    x.StartDate)
                .FirstOrDefault();

        if (selectedMembership is null)
        {
            selectedMembership =
                memberships
                    .Where(x =>
                        x.StartDate >
                            asOfDate &&
                        x.Status !=
                            MembershipStatus.Cancelled)
                    .OrderBy(x =>
                        x.StartDate)
                    .FirstOrDefault();
        }

        if (selectedMembership is null)
        {
            selectedMembership =
                memberships
                    .Where(x =>
                        x.StartDate <=
                        asOfDate)
                    .OrderByDescending(x =>
                        x.EndDate)
                    .FirstOrDefault();
        }

        MemberCurrentMembershipDto?
            currentMembership = null;

        if (selectedMembership is not null)
        {
            var effectiveStatus =
                selectedMembership
                    .GetEffectiveStatus(
                        asOfDate);

            var planName =
                planNames.TryGetValue(
                    selectedMembership.PlanId,
                    out var name)
                    ? name
                    : "Unknown Plan";

            currentMembership =
                new MemberCurrentMembershipDto(
                    selectedMembership.MembershipId,
                    selectedMembership.PlanId,
                    planName,
                    selectedMembership
                        .AgreedTerms
                        .PricePaid,
                    selectedMembership.StartDate,
                    selectedMembership.EndDate,
                    effectiveStatus,
                    selectedMembership
                        .Freezes
                        .Count,
                    selectedMembership
                        .AgreedTerms
                        .MaximumNumberOfFreezes,
                    selectedMembership
                        .GetTotalFrozenDays(),
                    selectedMembership
                        .AgreedTerms
                        .MaximumFreezeDays,
                    selectedMembership
                        .GuestPasses
                        .Count(x =>
                            x.UsedOn.HasValue),
                    selectedMembership
                        .AgreedTerms
                        .GuestPassQuota);
        }

        var membershipHistory =
            memberships
                .Select(membership =>
                {
                    var planName =
                        planNames.TryGetValue(
                            membership.PlanId,
                            out var name)
                            ? name
                            : "Unknown Plan";

                    return new MemberMembershipHistoryDto(
                        membership.MembershipId,
                        membership.PlanId,
                        planName,
                        membership.PurchaseDate,
                        membership.StartDate,
                        membership.EndDate,
                        membership.GetEffectiveStatus(
                            asOfDate),
                        membership
                            .AgreedTerms
                            .PricePaid,
                        membership
                            .AgreedTerms
                            .DurationInMonths,
                        membership
                            .AgreedTerms
                            .MaximumFreezeDays,
                        membership
                            .AgreedTerms
                            .MaximumNumberOfFreezes,
                        membership
                            .AgreedTerms
                            .GuestPassQuota,
                        membership
                            .AgreedTerms
                            .AccessScope);
                })
                .ToList();

        var branchNames =
            await _dbContext.Branches
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.BranchId,
                    x => x.BranchName,
                    cancellationToken);

        var checkIns =
            await _dbContext.CheckIns
                .AsNoTracking()
                .Where(x => x.MemberId == memberId)
                .OrderByDescending(x => x.CheckInDateTime)
                .Take(20)
                .Select(x => new
                {
                    x.BranchId,
                    x.Result,
                    x.RefusalReason,
                    x.CheckInDateTime
                })
                .ToListAsync(cancellationToken);

        var checkInActivities =
            checkIns
                .Select(x => new MemberRecentActivityDto(
                    "Check-In",
                    x.Result == CheckInResult.Admitted
                        ? "Facility Check-in"
                        : "Check-in refused",
                    x.Result == CheckInResult.Refused
                        ? x.RefusalReason
                        : branchNames.GetValueOrDefault(x.BranchId),
                    x.CheckInDateTime))
                .ToList();

        var attendedSessions =
            await _dbContext.Bookings
                .AsNoTracking()
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Status == BookingStatus.Attended)
                .Join(
                    _dbContext.ClassSessions.AsNoTracking(),
                    booking => booking.SessionId,
                    session => session.SessionId,
                    (booking, session) => session)
                .OrderByDescending(session => session.SessionDate)
                .ThenByDescending(session => session.StartTime)
                .Take(20)
                .ToListAsync(cancellationToken);

        var sessionTrainerIds =
            attendedSessions
                .Where(x => x.TrainerId.HasValue)
                .Select(x => x.TrainerId!.Value)
                .Distinct()
                .ToList();

        var trainerNames =
            await _dbContext.Trainers
                .AsNoTracking()
                .Where(x => sessionTrainerIds.Contains(x.TrainerId))
                .ToDictionaryAsync(
                    x => x.TrainerId,
                    x => x.TrainerName,
                    cancellationToken);

        var classActivities =
            attendedSessions
                .Select(session =>
                {
                    var trainerFirstName =
                        session.TrainerId.HasValue &&
                        trainerNames.TryGetValue(
                            session.TrainerId.Value,
                            out var trainerName)
                            ? trainerName.Split(' ')[0]
                            : null;

                    return new MemberRecentActivityDto(
                        "Class Attendance",
                        trainerFirstName is null
                            ? session.ClassName
                            : $"{session.ClassName} with {trainerFirstName}",
                        "Attended",
                        session.SessionDate.ToDateTime(
                            session.StartTime));
                });

        var recentActivities =
            checkInActivities
                .Concat(classActivities)
                .OrderByDescending(x => x.OccurredOn)
                .Take(20)
                .ToList();

        return new MemberProfileDto(
            member.MemberId,
            member.MembershipNumber,
            member.FullName,
            member.Email,
            member.Phone,
            member.Address,
            member.JoinedDate,
            member.Photo,
            member.HomeBranchId,
            branchName,
            memberStatus,
            currentMembership,
            membershipHistory,
            recentActivities);
    }

    private static MembershipStatus? GetMemberStatus(
        IReadOnlyCollection<Membership> memberships,
        DateOnly asOfDate)
    {
        if (memberships.Count == 0)
        {
            return null;
        }

        var coveringMembership =
            memberships
                .Where(x =>
                    x.StartDate <=
                        asOfDate &&
                    asOfDate <
                        x.EndDate)
                .OrderByDescending(x =>
                    x.StartDate)
                .FirstOrDefault();

        if (coveringMembership is not null)
        {
            var effectiveStatus =
                coveringMembership
                    .GetEffectiveStatus(
                        asOfDate);

            if (effectiveStatus ==
                MembershipStatus.Cancelled)
            {
                return MembershipStatus.Expired;
            }

            if (effectiveStatus ==
                MembershipStatus.Pending)
            {
                return null;
            }

            return effectiveStatus;
        }

        var hasFutureMembership =
            memberships.Any(x =>
                x.Status !=
                    MembershipStatus.Cancelled &&
                x.StartDate >
                    asOfDate);

        if (hasFutureMembership)
        {
            return null;
        }

        return MembershipStatus.Expired;
    }
}