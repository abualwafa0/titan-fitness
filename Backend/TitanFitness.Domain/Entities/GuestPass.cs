namespace TitanFitness.Domain.Entities;

public class GuestPass
{
    public int GuestPassId { get; private set; }

    public int MembershipId { get; private set; }

    public DateOnly IssuedOn { get; private set; }

    public DateOnly? UsedOn { get; private set; }

    public string? GuestName { get; private set; }

    private GuestPass()
    {
    }

    internal GuestPass(
        DateOnly issuedOn,
        string? guestName)
    {
        IssuedOn = issuedOn;

        SetGuestName(guestName);
    }

    internal void Use(DateOnly usedOn)
    {
        if (UsedOn.HasValue)
            throw new InvalidOperationException(
                "Guest pass has already been used.");

        if (usedOn < IssuedOn)
            throw new InvalidOperationException(
                "Guest pass cannot be used before it is issued.");

        UsedOn = usedOn;
    }

    private void SetGuestName(string? guestName)
    {
        if (string.IsNullOrWhiteSpace(guestName))
        {
            GuestName = null;
            return;
        }

        guestName = guestName.Trim();

        if (guestName.Length > 100)
            throw new ArgumentException(
                "Guest name cannot exceed 100 characters.",
                nameof(guestName));

        GuestName = guestName;
    }
}