using TitanFitness.Domain.Enums;

namespace TitanFitness.API.Contracts.Memberships;

public sealed class FreezeMembershipRequest
{
    public DateOnly StartDate { get; set; }

    public int DurationInMonths { get; set; }

    public FreezeReason Reason { get; set; }

    public string? AdditionalNotes { get; set; }
}