namespace TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;

public sealed record TrainerDirectoryItemDto(
    int TrainerId,
    string TrainerNumber,
    string TrainerName,
    string? Specialty,
    int BranchId,
    string BranchName,
    bool IsActive);