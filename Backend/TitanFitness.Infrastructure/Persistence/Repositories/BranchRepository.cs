using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class BranchRepository
    : IBranchRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public BranchRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Branch?> GetByIdAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Branches
            .Include(x => x.Studios)
            .FirstOrDefaultAsync(
                x => x.BranchId == branchId,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Branches
            .AnyAsync(
                x => x.BranchId == branchId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>>
        GetAllIdsAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .OrderBy(x => x.BranchId)
            .Select(x => x.BranchId)
            .ToListAsync(
                cancellationToken);
    }

    public async Task AddAsync(
        Branch branch,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Branches.AddAsync(
            branch,
            cancellationToken);
    }
}