using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.Entities;

public class Freeze
{
    public int FreezeId { get; private set; }

    public int MembershipId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public int DurationInMonths { get; private set; }

    public FreezeReason Reason { get; private set; }

    public string? AdditionalNotes { get; private set; }

    public DateTime RequestedOn { get; private set; }

    private Freeze()
    {
    }

    internal Freeze(
        DateOnly startDate,
        int durationInMonths,
        FreezeReason reason,
        string? additionalNotes,
        DateTime requestedOn)
    {
        if (durationInMonths is not (1 or 2 or 3))
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationInMonths),
                "Freeze duration must be 1, 2, or 3 months.");
        }

        if (!Enum.IsDefined(typeof(FreezeReason), reason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                "Invalid freeze reason.");
        }

        StartDate = startDate;
        DurationInMonths = durationInMonths;

        EndDate =
            startDate.AddMonths(durationInMonths);

        Reason = reason;
        RequestedOn = requestedOn;

        SetAdditionalNotes(additionalNotes);
    }

    public int GetFrozenDays()
    {
        return EndDate.DayNumber -
               StartDate.DayNumber;
    }

    internal bool Overlaps(
        DateOnly startDate,
        DateOnly endDate)
    {
        return StartDate < endDate &&
               startDate < EndDate;
    }

    private void SetAdditionalNotes(
        string? additionalNotes)
    {
        if (string.IsNullOrWhiteSpace(additionalNotes))
        {
            AdditionalNotes = null;
            return;
        }

        additionalNotes =
            additionalNotes.Trim();

        if (additionalNotes.Length > 500)
        {
            throw new ArgumentException(
                "Additional notes cannot exceed 500 characters.",
                nameof(additionalNotes));
        }

        AdditionalNotes = additionalNotes;
    }
}