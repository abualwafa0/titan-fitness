using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.Entities;

public class Plan
{
    private readonly List<PlanBranch> _availableBranches = new();

    public int PlanId { get; private set; }

    public string PlanName { get; private set; } = null!;

    public decimal Price { get; private set; }

    public int DurationInMonths { get; private set; }

    public int MaximumFreezeDays { get; private set; }

    public int MaximumNumberOfFreezes { get; private set; }

    public int GuestPassQuota { get; private set; }

    public AccessScope AccessScope { get; private set; }

    public bool IsPublished { get; private set; }

    public IReadOnlyCollection<PlanBranch> AvailableBranches =>
        _availableBranches.AsReadOnly();

    private Plan()
    {
    }

    public Plan(
        string planName,
        decimal price,
        int durationInMonths,
        int maximumFreezeDays,
        int maximumNumberOfFreezes,
        int guestPassQuota,
        AccessScope accessScope,
        bool isPublished)
    {
        SetPlanName(planName);
        SetPrice(price);
        SetDurationInMonths(durationInMonths);
        SetMaximumFreezeDays(maximumFreezeDays);
        SetMaximumNumberOfFreezes(maximumNumberOfFreezes);
        SetGuestPassQuota(guestPassQuota);
        SetAccessScope(accessScope);

        IsPublished = isPublished;
    }

    public void Update(
        string planName,
        decimal price,
        int durationInMonths,
        int maximumFreezeDays,
        int maximumNumberOfFreezes,
        int guestPassQuota,
        AccessScope accessScope,
        bool isPublished)
    {
        SetPlanName(planName);
        SetPrice(price);
        SetDurationInMonths(durationInMonths);
        SetMaximumFreezeDays(maximumFreezeDays);
        SetMaximumNumberOfFreezes(maximumNumberOfFreezes);
        SetGuestPassQuota(guestPassQuota);
        SetAccessScope(accessScope);

        IsPublished = isPublished;
    }

    public void SetAvailableBranches(
        IEnumerable<int> branchIds)
    {
        ArgumentNullException.ThrowIfNull(branchIds);

        var ids = branchIds
            .Distinct()
            .ToList();

        if (ids.Any(x => x <= 0))
        {
            throw new ArgumentException(
                "Branch ids must be greater than zero.",
                nameof(branchIds));
        }

        _availableBranches.Clear();

        foreach (var branchId in ids)
        {
            _availableBranches.Add(
                new PlanBranch(branchId));
        }
    }

    public bool IsAvailableAtBranch(
        int branchId)
    {
        if (branchId <= 0)
        {
            return false;
        }

        if (_availableBranches.Count == 0)
        {
            return true;
        }

        return _availableBranches.Any(
            x => x.BranchId == branchId);
    }

    private void SetPlanName(string planName)
    {
        if (string.IsNullOrWhiteSpace(planName))
        {
            throw new ArgumentException(
                "Plan name is required.",
                nameof(planName));
        }

        planName = planName.Trim();

        if (planName.Length > 60)
        {
            throw new ArgumentException(
                "Plan name cannot exceed 60 characters.",
                nameof(planName));
        }

        PlanName = planName;
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Price cannot be negative.");
        }

        if (decimal.Round(price, 2) != price)
        {
            throw new ArgumentException(
                "Price cannot have more than two decimal places.",
                nameof(price));
        }

        Price = price;
    }

    private void SetDurationInMonths(
        int durationInMonths)
    {
        if (durationInMonths <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationInMonths),
                "Duration must be greater than zero.");
        }

        DurationInMonths = durationInMonths;
    }

    private void SetMaximumFreezeDays(
        int maximumFreezeDays)
    {
        if (maximumFreezeDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumFreezeDays),
                "Maximum freeze days cannot be negative.");
        }

        MaximumFreezeDays = maximumFreezeDays;
    }

    private void SetMaximumNumberOfFreezes(
        int maximumNumberOfFreezes)
    {
        if (maximumNumberOfFreezes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumNumberOfFreezes),
                "Maximum number of freezes cannot be negative.");
        }

        MaximumNumberOfFreezes = maximumNumberOfFreezes;
    }

    private void SetGuestPassQuota(
        int guestPassQuota)
    {
        if (guestPassQuota < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(guestPassQuota),
                "Guest pass quota cannot be negative.");
        }

        GuestPassQuota = guestPassQuota;
    }

    private void SetAccessScope(
        AccessScope accessScope)
    {
        if (!Enum.IsDefined(
                typeof(AccessScope),
                accessScope))
        {
            throw new ArgumentOutOfRangeException(
                nameof(accessScope),
                "Invalid access scope.");
        }

        AccessScope = accessScope;
    }
}