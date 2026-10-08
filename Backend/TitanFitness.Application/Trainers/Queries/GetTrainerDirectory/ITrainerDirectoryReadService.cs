using TitanFitness.Application.Common.Models;

namespace TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;

public interface ITrainerDirectoryReadService
{
    Task<PagedResult<TrainerDirectoryItemDto>> GetAsync(
        string? search,
        IReadOnlyCollection<int>? branchIds,
        IReadOnlyCollection<string>? specialties,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetSpecialtiesAsync(
        CancellationToken cancellationToken = default);
}