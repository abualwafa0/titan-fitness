using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class MemberRepository
    : IMemberRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public MemberRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Member?> GetByIdAsync(
        int memberId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Members
            .FirstOrDefaultAsync(
                x => x.MemberId == memberId,
                cancellationToken);
    }

    public Task<Member?> GetByMembershipNumberAsync(
        string membershipNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Members
            .FirstOrDefaultAsync(
                x => x.MembershipNumber ==
                     membershipNumber,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(
        int memberId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Members
            .AnyAsync(
                x => x.MemberId == memberId,
                cancellationToken);
    }

    public Task<bool> MembershipNumberExistsAsync(
        string membershipNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Members
            .AnyAsync(
                x => x.MembershipNumber ==
                     membershipNumber,
                cancellationToken);
    }
    public async Task<string> GetNextMembershipNumberAsync(
    CancellationToken cancellationToken = default)
    {
        var numbers = await _dbContext.Members
            .AsNoTracking()
            .Select(x => x.MembershipNumber)
            .ToListAsync(cancellationToken);

        var highest = 1000;

        foreach (var number in numbers)
        {
            var digits = number.StartsWith(
                "TF-",
                StringComparison.OrdinalIgnoreCase)
                ? number[3..]
                : number;

            if (int.TryParse(digits, out var parsed) &&
                parsed > highest)
            {
                highest = parsed;
            }
        }

        return $"TF-{highest + 1:D4}";
    }
    public async Task AddAsync(
        Member member,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Members.AddAsync(
            member,
            cancellationToken);
    }
}