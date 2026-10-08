using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.Entities;

public class CheckIn
{
    public int CheckInId { get; private set; }

    public int MemberId { get; private set; }

    public int BranchId { get; private set; }

    public DateTime CheckInDateTime { get; private set; }

    public DateTime? CheckOutDateTime { get; private set; }

    public CheckInResult Result { get; private set; }

    public string? RefusalReason { get; private set; }

    public string? Notes { get; private set; }

    private CheckIn()
    {
    }

    public CheckIn(
        int memberId,
        int branchId,
        DateTime checkInDateTime,
        CheckInResult result,
        string? refusalReason,
        string? notes = null)
    {
        if (memberId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(memberId),
                "Member ID must be greater than zero.");

        if (branchId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch ID must be greater than zero.");

        if (!Enum.IsDefined(typeof(CheckInResult), result))
            throw new ArgumentOutOfRangeException(
                nameof(result),
                "Invalid check-in result.");

        if (result == CheckInResult.Admitted &&
            !string.IsNullOrWhiteSpace(refusalReason))
        {
            throw new ArgumentException(
                "An admitted check-in cannot have a refusal reason.",
                nameof(refusalReason));
        }

        if (result == CheckInResult.Refused &&
            string.IsNullOrWhiteSpace(refusalReason))
        {
            throw new ArgumentException(
                "A refused check-in must have a refusal reason.",
                nameof(refusalReason));
        }

        if (!string.IsNullOrWhiteSpace(refusalReason))
        {
            refusalReason = refusalReason.Trim();

            if (refusalReason.Length > 100)
                throw new ArgumentException(
                    "Refusal reason cannot exceed 100 characters.",
                    nameof(refusalReason));
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            notes = notes.Trim();

            if (notes.Length > 250)
                throw new ArgumentException(
                    "Notes cannot exceed 250 characters.",
                    nameof(notes));
        }

        MemberId = memberId;
        BranchId = branchId;
        CheckInDateTime = checkInDateTime;
        Result = result;

        RefusalReason =
            string.IsNullOrWhiteSpace(refusalReason)
                ? null
                : refusalReason;

        Notes =
            string.IsNullOrWhiteSpace(notes)
                ? null
                : notes;
    }

    public bool IsCurrentlyInside()
    {
        return Result == CheckInResult.Admitted &&
               CheckOutDateTime is null;
    }

    public void CheckOut(DateTime checkOutDateTime)
    {
        if (Result != CheckInResult.Admitted)
            throw new InvalidOperationException(
                "A refused check-in cannot be checked out.");

        if (CheckOutDateTime.HasValue)
            throw new InvalidOperationException(
                "This check-in has already been checked out.");

        if (checkOutDateTime < CheckInDateTime)
            throw new InvalidOperationException(
                "Check-out time cannot be before check-in time.");

        CheckOutDateTime = checkOutDateTime;
    }
}