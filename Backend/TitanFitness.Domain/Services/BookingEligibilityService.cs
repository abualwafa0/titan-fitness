using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Domain.Services;

public sealed class BookingEligibilityService
{
    private readonly IClassSessionRepository _classSessionRepository;

    public BookingEligibilityService(
        IClassSessionRepository classSessionRepository)
    {
        _classSessionRepository =
            classSessionRepository
            ?? throw new ArgumentNullException(
                nameof(classSessionRepository));
    }

    public async Task EnsureCanBookAsync(
        Member member,
        Membership? membership,
        DateOnly sessionDate,
        int sessionBranchId,
        TimeRange sessionTimeRange,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(sessionTimeRange);

        if (sessionBranchId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sessionBranchId),
                "Session branch ID must be greater than zero.");
        }

        if (membership is null)
        {
            throw new InvalidOperationException(
                "An active membership is required to book a class.");
        }

        if (membership.MemberId != member.MemberId)
        {
            throw new InvalidOperationException(
                "The membership does not belong to the selected member.");
        }

        var membershipStatus =
            membership.GetEffectiveStatus(
                sessionDate);

        if (membershipStatus !=
            MembershipStatus.Active)
        {
            throw new InvalidOperationException(
                "The member must have an active membership on the class date.");
        }

        if (membership.AgreedTerms.AccessScope ==
                AccessScope.HomeBranchOnly &&
            member.HomeBranchId !=
                sessionBranchId)
        {
            throw new InvalidOperationException(
                "The membership does not allow booking classes at this branch.");
        }

        var hasConflict =
            await _classSessionRepository
                .HasMemberBookingConflictAsync(
                    member.MemberId,
                    sessionTimeRange,
                    excludeSessionId,
                    cancellationToken);

        if (hasConflict)
        {
            throw new InvalidOperationException(
                "The member already has a booking that overlaps with this session.");
        }
    }
}