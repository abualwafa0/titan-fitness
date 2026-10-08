using TitanFitness.Domain.Entities;

namespace TitanFitness.Domain.Repositories;

public interface ITrainerRepository
{
    Task<Trainer?> GetByIdAsync(
        int trainerId,
        CancellationToken cancellationToken = default);

    Task<Trainer?> GetByTrainerNumberAsync(
        string trainerNumber,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int trainerId,
        CancellationToken cancellationToken = default);

    Task<bool> TrainerNumberExistsAsync(
        string trainerNumber,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludeTrainerId,
        CancellationToken cancellationToken = default);

    Task<string> GetNextTrainerNumberAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Trainer trainer,
        CancellationToken cancellationToken = default);
}