using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Branches;

public sealed class StudiosByBranchReadService
    : IStudiosByBranchReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public StudiosByBranchReadService(TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<StudioDto>> GetAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Studios
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.StudioName)
            .Select(x => new StudioDto(
                x.StudioId,
                x.StudioName,
                x.BranchId,
                x.Capacity))
            .ToListAsync(cancellationToken);
    }
}