namespace TitanFitness.Application.Trainers.Queries.GetTrainerDetails;

public interface ITrainerDetailsReadService
{
    Task<TrainerDetailsDto?> GetByIdAsync(
        int trainerId,
        CancellationToken cancellationToken = default);
}