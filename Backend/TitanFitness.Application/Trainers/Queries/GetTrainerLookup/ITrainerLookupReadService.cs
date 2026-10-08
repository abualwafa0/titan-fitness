namespace TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

public interface ITrainerLookupReadService
{
    Task<IReadOnlyCollection<TrainerLookupDto>> GetAsync(
        int branchId,
        string? search,
        CancellationToken cancellationToken = default);
}