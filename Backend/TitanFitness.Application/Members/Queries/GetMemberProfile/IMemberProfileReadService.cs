namespace TitanFitness.Application.Members.Queries.GetMemberProfile;

public interface IMemberProfileReadService
{
    Task<MemberProfileDto?> GetAsync(
        int memberId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);
}