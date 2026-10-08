namespace TitanFitness.Application.CheckIns.Commands.CheckOutMember;

public sealed record CheckOutMemberCommand(
    int MemberId);

public sealed record CheckOutMemberResult(
    int CheckInId,
    int MemberId,
    int BranchId,
    DateTime CheckInDateTime,
    DateTime CheckOutDateTime);