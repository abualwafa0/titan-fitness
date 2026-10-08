namespace TitanFitness.API.Contracts.ClassSessions;

public sealed class UpdateClassRequest
{
    public string ClassName { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public int? StudioId { get; set; }

    public int? TrainerId { get; set; }

    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationInMinutes { get; set; } = 45;

    public int? CapacityLimit { get; set; }

    public string? Description { get; set; }
}