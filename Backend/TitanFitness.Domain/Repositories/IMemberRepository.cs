using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(
        int memberId,
        CancellationToken cancellationToken = default);

    Task<Member?> GetByMembershipNumberAsync(
        string membershipNumber,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int memberId,
        CancellationToken cancellationToken = default);

    Task<bool> MembershipNumberExistsAsync(
        string membershipNumber,
        CancellationToken cancellationToken = default);
    Task<string> GetNextMembershipNumberAsync(
    CancellationToken cancellationToken = default);

    Task AddAsync(
        Member member,
        CancellationToken cancellationToken = default);
}