using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Branches.Queries.GetBranches;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Branches;

public sealed class BranchesReadService
    : IBranchesReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public BranchesReadService(TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<BranchDto>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .OrderBy(x => x.BranchName)
            .Select(x => new BranchDto(
                x.BranchId,
                x.BranchName,
                x.Address,
                x.OpeningTime,
                x.ClosingTime))
            .ToListAsync(cancellationToken);
    }
}