namespace TitanFitness.Domain.ValueObjects;

public sealed class TimeRange : IEquatable<TimeRange>
{
    public DateTime Start { get; }

    public DateTime End { get; }

    public TimeRange(
        DateTime start,
        DateTime end)
    {
        if (end <= start)
            throw new ArgumentException(
                "End time must be after start time.",
                nameof(end));

        Start = start;
        End = end;
    }

    public bool Overlaps(TimeRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Start < other.End &&
               other.Start < End;
    }

    public bool Equals(TimeRange? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Start == other.Start &&
               End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TimeRange);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Start,
            End);
    }

    public static bool operator ==(
        TimeRange? left,
        TimeRange? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        TimeRange? left,
        TimeRange? right)
    {
        return !Equals(left, right);
    }
}