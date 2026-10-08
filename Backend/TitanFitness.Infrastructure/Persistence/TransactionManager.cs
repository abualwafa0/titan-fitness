using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Abstractions.Persistence;

namespace TitanFitness.Infrastructure.Persistence;

public sealed class TransactionManager
    : ITransactionManager
{
    private readonly TitanFitnessDbContext _dbContext;

    public TransactionManager(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy =
            _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                try
                {
                    await operation(
                        cancellationToken);

                    await transaction.CommitAsync(
                        cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    throw;
                }
            });
    }
}