using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.Services;

public sealed class CheckInEligibilityService
{
    public CheckInEligibilityResult Evaluate(
        Member member,
        Membership? membership,
        int branchId,
        DateTime checkInDateTime)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (branchId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(branchId));

        if (membership is null)
        {
            return CheckInEligibilityResult.Refused(
                "No membership");
        }

        if (membership.MemberId != member.MemberId)
        {
            throw new InvalidOperationException(
                "The membership does not belong to the selected member.");
        }

        var date = DateOnly.FromDateTime(checkInDateTime);

        var status = membership.GetEffectiveStatus(date);

        switch (status)
        {
            case MembershipStatus.Pending:
                return CheckInEligibilityResult.Refused(
                    "Membership not yet started");

            case MembershipStatus.Expired:
                return CheckInEligibilityResult.Refused(
                    "Membership expired");

            case MembershipStatus.Frozen:
                return CheckInEligibilityResult.Refused(
                    "Membership frozen");

            case MembershipStatus.Cancelled:
                return CheckInEligibilityResult.Refused(
                    "Membership cancelled");

            case MembershipStatus.Active:
                break;

            default:
                return CheckInEligibilityResult.Refused(
                    "Membership is not active");
        }

        if (membership.AgreedTerms.AccessScope
                == AccessScope.HomeBranchOnly
            && member.HomeBranchId != branchId)
        {
            return CheckInEligibilityResult.Refused(
                "Wrong branch");
        }

        return CheckInEligibilityResult.Admitted();
    }
}

public sealed record CheckInEligibilityResult(
    bool IsAdmitted,
    string? RefusalReason)
{
    public static CheckInEligibilityResult Admitted()
    {
        return new CheckInEligibilityResult(
            true,
            null);
    }

    public static CheckInEligibilityResult Refused(
        string refusalReason)
    {
        if (string.IsNullOrWhiteSpace(refusalReason))
            throw new ArgumentException(
                "Refusal reason is required.",
                nameof(refusalReason));

        return new CheckInEligibilityResult(
            false,
            refusalReason.Trim());
    }
}