using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Trainers.Queries.GetTrainerDetails;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Trainers;

public sealed class TrainerDetailsReadService
    : ITrainerDetailsReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public TrainerDetailsReadService(TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TrainerDetailsDto?> GetByIdAsync(
        int trainerId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from trainer in _dbContext.Trainers.AsNoTracking()
            join branch in _dbContext.Branches.AsNoTracking()
                on trainer.BranchId equals branch.BranchId
            where trainer.TrainerId == trainerId
            select new TrainerDetailsDto(
                trainer.TrainerId,
                trainer.TrainerNumber,
                trainer.TrainerName,
                trainer.Specialty,
                trainer.Email,
                trainer.Phone,
                trainer.BranchId,
                branch.BranchName,
                trainer.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}