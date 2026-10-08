namespace TitanFitness.Application.Trainers.Commands.UpdateTrainer;

public sealed record UpdateTrainerCommand(
    int TrainerId,
    string TrainerName,
    string? Specialty,
    string? Email,
    string? Phone,
    int BranchId,
    bool IsActive);