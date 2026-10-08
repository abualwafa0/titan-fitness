using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.ValueObjects;

public sealed class AgreedTerms : IEquatable<AgreedTerms>
{
    public decimal PricePaid { get; private set; }

    public int DurationInMonths { get; private set; }

    public int MaximumFreezeDays { get; private set; }

    public int MaximumNumberOfFreezes { get; private set; }

    public int GuestPassQuota { get; private set; }

    public AccessScope AccessScope { get; private set; }

    private AgreedTerms()
    {
    }

    public AgreedTerms(
        decimal pricePaid,
        int durationInMonths,
        int maximumFreezeDays,
        int maximumNumberOfFreezes,
        int guestPassQuota,
        AccessScope accessScope)
    {
        if (pricePaid < 0)
            throw new ArgumentOutOfRangeException(
                nameof(pricePaid),
                "Price paid cannot be negative.");

        if (decimal.Round(pricePaid, 2) != pricePaid)
            throw new ArgumentException(
                "Price paid cannot have more than two decimal places.",
                nameof(pricePaid));

        if (durationInMonths <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(durationInMonths),
                "Duration must be greater than zero.");

        if (maximumFreezeDays < 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumFreezeDays),
                "Maximum freeze days cannot be negative.");

        if (maximumNumberOfFreezes < 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumNumberOfFreezes),
                "Maximum number of freezes cannot be negative.");

        if (guestPassQuota < 0)
            throw new ArgumentOutOfRangeException(
                nameof(guestPassQuota),
                "Guest pass quota cannot be negative.");

        if (!Enum.IsDefined(typeof(AccessScope), accessScope))
            throw new ArgumentOutOfRangeException(
                nameof(accessScope),
                "Invalid access scope.");

        PricePaid = pricePaid;
        DurationInMonths = durationInMonths;
        MaximumFreezeDays = maximumFreezeDays;
        MaximumNumberOfFreezes = maximumNumberOfFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
    }

    public bool Equals(AgreedTerms? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return PricePaid == other.PricePaid
            && DurationInMonths == other.DurationInMonths
            && MaximumFreezeDays == other.MaximumFreezeDays
            && MaximumNumberOfFreezes == other.MaximumNumberOfFreezes
            && GuestPassQuota == other.GuestPassQuota
            && AccessScope == other.AccessScope;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AgreedTerms);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            PricePaid,
            DurationInMonths,
            MaximumFreezeDays,
            MaximumNumberOfFreezes,
            GuestPassQuota,
            AccessScope);
    }

    public static bool operator ==(
        AgreedTerms? left,
        AgreedTerms? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        AgreedTerms? left,
        AgreedTerms? right)
    {
        return !Equals(left, right);
    }
}