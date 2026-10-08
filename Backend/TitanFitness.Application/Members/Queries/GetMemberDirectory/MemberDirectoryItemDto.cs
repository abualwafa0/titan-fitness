using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Members.Queries.GetMemberDirectory;

public sealed record MemberDirectoryItemDto(
    int MemberId,
    string FullName,
    string MembershipNumber,
    MembershipStatus? Status,
    int HomeBranchId,
    string BranchName,
    DateTime? LastVisit,
    int? MembershipId,
    int? FreezesRemaining);