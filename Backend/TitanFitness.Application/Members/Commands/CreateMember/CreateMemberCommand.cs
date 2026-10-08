namespace TitanFitness.Application.Members.Commands.CreateMember;

public sealed record CreateMemberCommand(
    string? MembershipNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    DateOnly JoinedDate,
    int HomeBranchId,
    Stream? PhotoStream = null,
    string? PhotoFileName = null);

public sealed record CreateMemberResult(
    int MemberId,
    string MembershipNumber);