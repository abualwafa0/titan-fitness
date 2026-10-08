namespace TitanFitness.Application.Members.Queries.SearchMembersLookup;

public interface IMemberLookupReadService
{
    Task<IReadOnlyCollection<MemberLookupDto>> SearchAsync(
        string? search,
        DateOnly asOfDate,
        int limit,
        CancellationToken cancellationToken = default);
}