using TitanFitness.Domain.Enums;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Domain.Entities;

public class Membership
{
    private readonly List<Freeze> _freezes = new();
    private readonly List<GuestPass> _guestPasses = new();

    public int MembershipId { get; private set; }

    public int MemberId { get; private set; }

    public int PlanId { get; private set; }

    public DateTime PurchaseDate { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public MembershipStatus Status { get; private set; }

    public AgreedTerms AgreedTerms { get; private set; } = null!;

    public IReadOnlyCollection<Freeze> Freezes =>
        _freezes.AsReadOnly();

    public IReadOnlyCollection<GuestPass> GuestPasses =>
        _guestPasses.AsReadOnly();

    private Membership()
    {
    }

    private Membership(
        int memberId,
        int planId,
        DateTime purchaseDate,
        DateOnly startDate,
        DateOnly endDate,
        MembershipStatus status,
        AgreedTerms agreedTerms)
    {
        if (memberId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(memberId),
                "Member ID must be greater than zero.");
        }

        if (planId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planId),
                "Plan ID must be greater than zero.");
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException(
                "Membership end date must be after its start date.");
        }

        MemberId = memberId;
        PlanId = planId;
        PurchaseDate = purchaseDate;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        AgreedTerms = agreedTerms
            ?? throw new ArgumentNullException(nameof(agreedTerms));
    }

    public static Membership Purchase(
        int memberId,
        Plan plan,
        DateTime purchaseDate,
        DateOnly startDate)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (!plan.IsPublished)
        {
            throw new InvalidOperationException(
                "An unpublished plan cannot be purchased.");
        }

        var agreedTerms = new AgreedTerms(
            plan.Price,
            plan.DurationInMonths,
            plan.MaximumFreezeDays,
            plan.MaximumNumberOfFreezes,
            plan.GuestPassQuota,
            plan.AccessScope);

        var endDate =
            startDate.AddMonths(
                agreedTerms.DurationInMonths);

        var purchaseDateOnly =
            DateOnly.FromDateTime(purchaseDate);

        var initialStatus =
            startDate > purchaseDateOnly
                ? MembershipStatus.Pending
                : MembershipStatus.Active;

        return new Membership(
            memberId,
            plan.PlanId,
            purchaseDate,
            startDate,
            endDate,
            initialStatus,
            agreedTerms);
    }

    public MembershipStatus GetEffectiveStatus(
        DateOnly date)
    {
        if (Status == MembershipStatus.Cancelled)
        {
            return MembershipStatus.Cancelled;
        }

        if (date < StartDate)
        {
            return MembershipStatus.Pending;
        }

        if (date >= EndDate)
        {
            return MembershipStatus.Expired;
        }

        if (IsFrozenOn(date))
        {
            return MembershipStatus.Frozen;
        }

        return MembershipStatus.Active;
    }

    public bool IsFrozenOn(
        DateOnly date)
    {
        return _freezes.Any(
            freeze =>
                date >= freeze.StartDate &&
                date < freeze.EndDate);
    }

    public int GetTotalFrozenDays()
    {
        return _freezes.Sum(
            freeze => freeze.GetFrozenDays());
    }

    public int GetRemainingFreezeDays()
    {
        return Math.Max(
            0,
            AgreedTerms.MaximumFreezeDays -
            GetTotalFrozenDays());
    }

    public int GetRemainingNumberOfFreezes()
    {
        return Math.Max(
            0,
            AgreedTerms.MaximumNumberOfFreezes -
            _freezes.Count);
    }

    public int GetIssuedGuestPassCount()
    {
        return _guestPasses.Count;
    }

    public int GetRemainingGuestPassCount()
    {
        return Math.Max(
            0,
            AgreedTerms.GuestPassQuota -
            _guestPasses.Count);
    }

    public Freeze AddFreeze(
        DateOnly startDate,
        int durationInMonths,
        FreezeReason reason,
        string? additionalNotes,
        DateTime requestedOn)
    {
        var requestDate =
            DateOnly.FromDateTime(requestedOn);

        if (Status == MembershipStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "A cancelled membership cannot be frozen.");
        }

        if (startDate < requestDate)
        {
            throw new InvalidOperationException(
                "A freeze cannot begin in the past.");
        }

        if (startDate < StartDate)
        {
            throw new InvalidOperationException(
                "A freeze cannot begin before the membership starts.");
        }

        if (startDate >= EndDate)
        {
            throw new InvalidOperationException(
                "A freeze must begin before the membership ends.");
        }

        if (_freezes.Count >=
            AgreedTerms.MaximumNumberOfFreezes)
        {
            throw new InvalidOperationException(
                "The maximum number of freezes has been reached.");
        }

        var freeze = new Freeze(
            startDate,
            durationInMonths,
            reason,
            additionalNotes,
            requestedOn);

        if (freeze.EndDate > EndDate)
        {
            throw new InvalidOperationException(
                "The freeze cannot run past the membership end date.");
        }

        if (_freezes.Any(
            existing =>
                existing.Overlaps(
                    freeze.StartDate,
                    freeze.EndDate)))
        {
            throw new InvalidOperationException(
                "The freeze overlaps an existing freeze.");
        }

        var projectedFrozenDays =
            GetTotalFrozenDays() +
            freeze.GetFrozenDays();

        if (projectedFrozenDays >
            AgreedTerms.MaximumFreezeDays)
        {
            throw new InvalidOperationException(
                "The maximum allowed freeze days would be exceeded.");
        }

        _freezes.Add(freeze);

        EndDate = EndDate.AddDays(
            freeze.GetFrozenDays());

        return freeze;
    }

    public GuestPass IssueGuestPass(
        DateOnly issuedOn,
        string? guestName)
    {
        if (Status == MembershipStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "A cancelled membership cannot issue guest passes.");
        }

        if (_guestPasses.Count >=
            AgreedTerms.GuestPassQuota)
        {
            throw new InvalidOperationException(
                "The guest pass quota has been reached.");
        }

        var guestPass = new GuestPass(
            issuedOn,
            guestName);

        _guestPasses.Add(guestPass);

        return guestPass;
    }

    public void UseGuestPass(
        int guestPassId,
        DateOnly usedOn)
    {
        var guestPass =
            _guestPasses.FirstOrDefault(
                x => x.GuestPassId == guestPassId);

        if (guestPass is null)
        {
            throw new InvalidOperationException(
                "Guest pass was not found in this membership.");
        }

        guestPass.Use(usedOn);
    }

    public void Cancel(
        DateOnly cancellationDate)
    {
        if (Status == MembershipStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "The membership is already cancelled.");
        }

        if (cancellationDate >= EndDate)
        {
            throw new InvalidOperationException(
                "An expired membership cannot be cancelled.");
        }

        Status = MembershipStatus.Cancelled;
    }
}