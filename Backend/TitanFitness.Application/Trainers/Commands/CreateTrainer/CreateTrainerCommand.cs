namespace TitanFitness.Application.Trainers.Commands.CreateTrainer;

public sealed record CreateTrainerCommand(
    string TrainerName,
    string? Specialty,
    string? Email,
    string? Phone,
    int BranchId,
    bool IsActive);

public sealed record CreateTrainerResult(
    int TrainerId,
    string TrainerNumber);