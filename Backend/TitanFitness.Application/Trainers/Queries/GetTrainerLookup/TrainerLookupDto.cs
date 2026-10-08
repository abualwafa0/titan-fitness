namespace TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

public sealed record TrainerLookupDto(
    int TrainerId,
    string TrainerNumber,
    string TrainerName,
    string? Specialty);