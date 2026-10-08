using TitanFitness.Application.Abstractions.Persistence;

namespace TitanFitness.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TitanFitnessDbContext _dbContext;

    public UnitOfWork(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}