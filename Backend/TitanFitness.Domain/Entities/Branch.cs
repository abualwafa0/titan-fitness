namespace TitanFitness.Domain.Entities;

public class Branch
{
    private readonly List<Studio> _studios = new();

    public int BranchId { get; private set; }

    public string BranchName { get; private set; } = null!;

    public string? Address { get; private set; }

    public TimeOnly OpeningTime { get; private set; }

    public TimeOnly ClosingTime { get; private set; }

    public IReadOnlyCollection<Studio> Studios => _studios.AsReadOnly();

    private Branch()
    {
    }

    public Branch(
        string branchName,
        string? address,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        SetBranchName(branchName);
        SetAddress(address);

        OpeningTime = openingTime;
        ClosingTime = closingTime;
    }

    public void Update(
        string branchName,
        string? address,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        SetBranchName(branchName);
        SetAddress(address);

        OpeningTime = openingTime;
        ClosingTime = closingTime;
    }

    public Studio AddStudio(
        string studioName,
        int capacity)
    {
        var studio = new Studio(
            studioName,
            capacity);

        _studios.Add(studio);

        return studio;
    }

    public void UpdateStudio(
        int studioId,
        string studioName,
        int capacity)
    {
        Studio? studio = _studios.FirstOrDefault(
            x => x.StudioId == studioId);

        if (studio is null)
            throw new InvalidOperationException(
                "Studio was not found in this branch.");

        studio.Update(
            studioName,
            capacity);
    }

    private void SetBranchName(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
            throw new ArgumentException(
                "Branch name is required.",
                nameof(branchName));

        branchName = branchName.Trim();

        if (branchName.Length > 50)
            throw new ArgumentException(
                "Branch name cannot exceed 50 characters.",
                nameof(branchName));

        BranchName = branchName;
    }

    private void SetAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            Address = null;
            return;
        }

        address = address.Trim();

        if (address.Length > 200)
            throw new ArgumentException(
                "Address cannot exceed 200 characters.",
                nameof(address));

        Address = address;
    }
}