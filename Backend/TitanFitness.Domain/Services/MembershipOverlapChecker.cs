using TitanFitness.Domain.Repositories;

namespace TitanFitness.Domain.Services;

public sealed class MembershipOverlapChecker
{
    private readonly IMembershipRepository _membershipRepository;

    public MembershipOverlapChecker(
        IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository
            ?? throw new ArgumentNullException(
                nameof(membershipRepository));
    }

    public async Task EnsureNoOverlapAsync(
        int memberId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludeMembershipId = null,
        CancellationToken cancellationToken = default)
    {
        if (memberId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(memberId));

        if (endDate <= startDate)
            throw new ArgumentException(
                "Membership end date must be after start date.",
                nameof(endDate));

        var hasOverlap =
            await _membershipRepository.HasOverlappingMembershipAsync(
                memberId,
                startDate,
                endDate,
                excludeMembershipId,
                cancellationToken);

        if (hasOverlap)
            throw new InvalidOperationException(
                "The member already has a membership covering part of this period.");
    }
}