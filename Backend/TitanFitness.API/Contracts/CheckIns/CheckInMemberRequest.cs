namespace TitanFitness.API.Contracts.CheckIns;

public sealed class CheckInMemberRequest
{
    public int MemberId { get; set; }

    public int BranchId { get; set; }

    public DateTime? CheckInDateTime { get; set; }

    public string? Notes { get; set; }
}