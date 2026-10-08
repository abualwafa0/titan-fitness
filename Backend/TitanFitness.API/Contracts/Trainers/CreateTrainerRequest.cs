namespace TitanFitness.API.Contracts.Trainers;

public sealed class CreateTrainerRequest
{
    public string TrainerName { get; set; } = string.Empty;

    public string? Specialty { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int BranchId { get; set; }

    public bool IsActive { get; set; } = true;
}