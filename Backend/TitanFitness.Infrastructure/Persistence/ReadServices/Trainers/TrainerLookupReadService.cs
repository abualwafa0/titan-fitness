using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Trainers;

public sealed class TrainerLookupReadService
    : ITrainerLookupReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public TrainerLookupReadService(TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TrainerLookupDto>> GetAsync(
        int branchId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Trainers
            .AsNoTracking()
            .Where(x =>
                x.BranchId == branchId &&
                x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.TrainerName.Contains(term) ||
                x.TrainerNumber.Contains(term) ||
                (x.Specialty != null &&
                 x.Specialty.Contains(term)));
        }

        return await query
            .OrderBy(x => x.TrainerName)
            .Select(x => new TrainerLookupDto(
                x.TrainerId,
                x.TrainerNumber,
                x.TrainerName,
                x.Specialty))
            .ToListAsync(cancellationToken);
    }
}