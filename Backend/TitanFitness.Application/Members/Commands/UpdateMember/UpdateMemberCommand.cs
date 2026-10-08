namespace TitanFitness.Application.Members.Commands.UpdateMember;

public sealed record UpdateMemberCommand(
    int MemberId,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    DateOnly JoinedDate,
    int HomeBranchId,
    Stream? PhotoStream = null,
    string? PhotoFileName = null);