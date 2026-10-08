using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Members.Queries.SearchMembersLookup;

public sealed record MemberLookupDto(
    int MemberId,
    string FullName,
    string MembershipNumber,
    string? Photo,
    MembershipStatus? Status);