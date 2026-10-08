namespace TitanFitness.Domain.Entities;

public class Studio
{
    public int StudioId { get; private set; }

    public string StudioName { get; private set; } = null!;

    public int BranchId { get; private set; }

    public int Capacity { get; private set; }

    private Studio()
    {
    }

    internal Studio(
        string studioName,
        int capacity)
    {
        SetStudioName(studioName);
        SetCapacity(capacity);
    }

    internal void Update(
        string studioName,
        int capacity)
    {
        SetStudioName(studioName);
        SetCapacity(capacity);
    }

    private void SetStudioName(string studioName)
    {
        if (string.IsNullOrWhiteSpace(studioName))
            throw new ArgumentException(
                "Studio name is required.",
                nameof(studioName));

        studioName = studioName.Trim();

        if (studioName.Length > 50)
            throw new ArgumentException(
                "Studio name cannot exceed 50 characters.",
                nameof(studioName));

        StudioName = studioName;
    }

    private void SetCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Studio capacity must be greater than zero.");

        Capacity = capacity;
    }
}