using TitanFitness.Application.Common.Models;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Members.Queries.GetMemberDirectory;

public interface IMemberDirectoryReadService
{
    Task<PagedResult<MemberDirectoryItemDto>> GetAsync(
        string? search,
        IReadOnlyCollection<int>? branchIds,
        IReadOnlyCollection<MembershipStatus>? statuses,
        string? sortBy,
        string? sortDirection,
        DateOnly asOfDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}