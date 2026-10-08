namespace TitanFitness.Application.Members.Queries.SearchMembersLookup;

public sealed class SearchMembersLookupQueryHandler
{
    private readonly IMemberLookupReadService _readService;

    public SearchMembersLookupQueryHandler(
        IMemberLookupReadService readService)
    {
        _readService = readService;
    }

    public async Task<IReadOnlyCollection<MemberLookupDto>> Handle(
        SearchMembersLookupQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Limit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.Limit),
                "Limit must be greater than zero.");
        }

        if (query.Limit > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.Limit),
                "Limit cannot exceed 100.");
        }

        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        var today = DateOnly.FromDateTime(DateTime.Now);

        return await _readService.SearchAsync(
            search,
            today,
            query.Limit,
            cancellationToken);
    }
}