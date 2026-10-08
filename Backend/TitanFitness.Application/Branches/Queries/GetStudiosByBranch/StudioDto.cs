namespace TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

public sealed record StudioDto(
    int StudioId,
    string StudioName,
    int BranchId,
    int Capacity);