using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class TrainerRepository
    : ITrainerRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public TrainerRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Trainer?> GetByIdAsync(
        int trainerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Trainers
            .FirstOrDefaultAsync(
                x => x.TrainerId == trainerId,
                cancellationToken);
    }

    public Task<Trainer?> GetByTrainerNumberAsync(
        string trainerNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Trainers
            .FirstOrDefaultAsync(
                x => x.TrainerNumber == trainerNumber,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(
        int trainerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Trainers
            .AnyAsync(
                x => x.TrainerId == trainerId,
                cancellationToken);
    }

    public Task<bool> TrainerNumberExistsAsync(
        string trainerNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Trainers
            .AnyAsync(
                x => x.TrainerNumber == trainerNumber,
                cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludeTrainerId,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLower();

        return _dbContext.Trainers
            .AnyAsync(
                x => x.Email != null &&
                     x.Email.ToLower() == normalized &&
                     (!excludeTrainerId.HasValue ||
                      x.TrainerId != excludeTrainerId.Value),
                cancellationToken);
    }

    public async Task<string> GetNextTrainerNumberAsync(
        CancellationToken cancellationToken = default)
    {
        var numbers = await _dbContext.Trainers
            .AsNoTracking()
            .Select(x => x.TrainerNumber)
            .ToListAsync(cancellationToken);

        var highest = 1000;

        foreach (var number in numbers)
        {
            var digits = number.StartsWith(
                "TR-",
                StringComparison.OrdinalIgnoreCase)
                ? number[3..]
                : number;

            if (int.TryParse(digits, out var parsed) &&
                parsed > highest)
            {
                highest = parsed;
            }
        }

        return $"TR-{highest + 1:D4}";
    }

    public async Task AddAsync(
        Trainer trainer,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Trainers.AddAsync(
            trainer,
            cancellationToken);
    }
}