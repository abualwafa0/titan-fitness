namespace TitanFitness.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}