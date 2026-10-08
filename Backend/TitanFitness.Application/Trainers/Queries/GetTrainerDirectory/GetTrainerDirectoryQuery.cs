namespace TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;

public sealed record GetTrainerDirectoryQuery(
    string? Search,
    IReadOnlyCollection<int>? BranchIds,
    IReadOnlyCollection<string>? Specialties,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    int PageNumber = 1,
    int PageSize = 10);