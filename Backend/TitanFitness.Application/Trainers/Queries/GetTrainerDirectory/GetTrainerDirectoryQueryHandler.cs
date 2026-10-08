using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Application.Common.Models;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;

public sealed class GetTrainerDirectoryQueryHandler
{
    private readonly ITrainerDirectoryReadService _readService;
    private readonly IBranchRepository _branchRepository;

    public GetTrainerDirectoryQueryHandler(
        ITrainerDirectoryReadService readService,
        IBranchRepository branchRepository)
    {
        _readService = readService;
        _branchRepository = branchRepository;
    }

    public async Task<PagedResult<TrainerDirectoryItemDto>> Handle(
        GetTrainerDirectoryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.PageNumber <= 0)
        {
            throw new ArgumentException(
                "Page number must be greater than zero.");
        }

        if (query.PageSize <= 0)
        {
            throw new ArgumentException(
                "Page size must be greater than zero.");
        }

        if (query.BranchIds is not null)
        {
            foreach (var branchId in query.BranchIds.Distinct())
            {
                var branchExists = await _branchRepository.ExistsAsync(
                    branchId,
                    cancellationToken);

                if (!branchExists)
                {
                    throw new NotFoundException(
                        $"Branch with id {branchId} was not found.");
                }
            }
        }

        return await _readService.GetAsync(
            query.Search,
            query.BranchIds,
            query.Specialties,
            query.IsActive,
            query.SortBy,
            query.SortDirection,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetSpecialties(
        CancellationToken cancellationToken = default)
    {
        return await _readService.GetSpecialtiesAsync(
            cancellationToken);
    }
}