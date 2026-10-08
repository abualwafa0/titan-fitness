using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.CheckIns.Commands.CheckInMember;

public sealed record CheckInMemberCommand(
    int MemberId,
    int BranchId,
    DateTime? CheckInDateTime = null,
    string? Notes = null);

public sealed record CheckInPreviewResult(
    int MemberId,
    int BranchId,
    DateTime EvaluatedAt,
    bool IsAdmitted,
    string? RefusalReason);

public sealed record CheckInMemberResult(
    int CheckInId,
    int MemberId,
    int BranchId,
    DateTime CheckInDateTime,
    CheckInResult Result,
    string? RefusalReason);