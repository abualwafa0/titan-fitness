namespace TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

public sealed record GetTrainerLookupQuery(
    int BranchId,
    string? Search = null);