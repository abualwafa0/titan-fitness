using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Members.Queries.GetMemberDirectory;

public sealed record GetMemberDirectoryQuery(
    string? Search = null,
    IReadOnlyCollection<int>? BranchIds = null,
    IReadOnlyCollection<MembershipStatus>? Statuses = null,
    string? SortBy = null,
    string? SortDirection = null,
    int PageNumber = 1,
    int PageSize = 20);