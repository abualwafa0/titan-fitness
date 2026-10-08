namespace TitanFitness.API.Contracts.Members;

public sealed class UpdateMemberRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateOnly JoinedDate { get; set; }

    public int HomeBranchId { get; set; }

    public IFormFile? Photo { get; set; }
}