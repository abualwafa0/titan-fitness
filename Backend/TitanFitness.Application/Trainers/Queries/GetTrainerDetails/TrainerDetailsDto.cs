namespace TitanFitness.Application.Trainers.Queries.GetTrainerDetails;

public sealed record TrainerDetailsDto(
    int TrainerId,
    string TrainerNumber,
    string TrainerName,
    string? Specialty,
    string? Email,
    string? Phone,
    int BranchId,
    string BranchName,
    bool IsActive);