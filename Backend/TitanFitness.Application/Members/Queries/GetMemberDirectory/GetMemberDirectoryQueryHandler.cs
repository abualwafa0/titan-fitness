using TitanFitness.Application.Common.Models;

namespace TitanFitness.Application.Members.Queries.GetMemberDirectory;

public sealed class GetMemberDirectoryQueryHandler
{
    private readonly IMemberDirectoryReadService _readService;

    public GetMemberDirectoryQueryHandler(
        IMemberDirectoryReadService readService)
    {
        _readService = readService;
    }

    public async Task<PagedResult<MemberDirectoryItemDto>> Handle(
        GetMemberDirectoryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.BranchIds is not null &&
            query.BranchIds.Any(x => x <= 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.BranchIds),
                "Branch ID must be greater than zero.");
        }

        if (query.PageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageNumber),
                "Page number must be greater than zero.");
        }

        if (query.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.PageSize),
                "Page size must be greater than zero.");
        }

        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        var today = DateOnly.FromDateTime(DateTime.Now);

        return await _readService.GetAsync(
            search,
            query.BranchIds,
            query.Statuses,
            query.SortBy,
            query.SortDirection,
            today,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}