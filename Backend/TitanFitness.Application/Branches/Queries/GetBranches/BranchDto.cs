namespace TitanFitness.Application.Branches.Queries.GetBranches;

public sealed record BranchDto(
    int BranchId,
    string BranchName,
    string? Address,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime);