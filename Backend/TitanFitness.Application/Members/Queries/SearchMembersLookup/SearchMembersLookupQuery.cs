namespace TitanFitness.Application.Members.Queries.SearchMembersLookup;

public sealed record SearchMembersLookupQuery(
    string? Search,
    int Limit = 20);