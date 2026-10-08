using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Models;
using TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Trainers;

public sealed class TrainerDirectoryReadService
    : ITrainerDirectoryReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public TrainerDirectoryReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TrainerDirectoryItemDto>> GetAsync(
        string? search,
        IReadOnlyCollection<int>? branchIds,
        IReadOnlyCollection<string>? specialties,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from trainer in _dbContext.Trainers.AsNoTracking()
            join branch in _dbContext.Branches.AsNoTracking()
                on trainer.BranchId equals branch.BranchId
            select new
            {
                Trainer = trainer,
                BranchName = branch.BranchName
            };

        if (branchIds is not null && branchIds.Count > 0)
        {
            var branchIdList = branchIds.Distinct().ToList();

            query = query.Where(x =>
                branchIdList.Contains(x.Trainer.BranchId));
        }

        if (specialties is not null && specialties.Count > 0)
        {
            var specialtyList = specialties
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            if (specialtyList.Count > 0)
            {
                query = query.Where(x =>
                    x.Trainer.Specialty != null &&
                    specialtyList.Contains(x.Trainer.Specialty));
            }
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.Trainer.IsActive == isActive.Value);
        }

        var rows = await query
            .Select(x => new TrainerDirectoryItemDto(
                x.Trainer.TrainerId,
                x.Trainer.TrainerNumber,
                x.Trainer.TrainerName,
                x.Trainer.Specialty,
                x.Trainer.BranchId,
                x.BranchName,
                x.Trainer.IsActive))
            .ToListAsync(cancellationToken);

        IEnumerable<TrainerDirectoryItemDto> filteredRows = rows;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            filteredRows = filteredRows.Where(x =>
                Contains(x.TrainerName, term) ||
                Contains(x.TrainerNumber, term) ||
                Contains(x.Specialty, term) ||
                Contains(x.BranchName, term) ||
                Contains(x.IsActive ? "Active" : "Inactive", term));
        }

        var ordered = Sort(filteredRows, sortBy, sortDirection);

        var filteredList = ordered.ToList();

        var totalCount = filteredList.Count;

        var items = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<TrainerDirectoryItemDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<IReadOnlyCollection<string>> GetSpecialtiesAsync(
        CancellationToken cancellationToken = default)
    {
        var values = await _dbContext.Trainers
            .AsNoTracking()
            .Where(x => x.Specialty != null && x.Specialty != "")
            .Select(x => x.Specialty!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return values
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IOrderedEnumerable<TrainerDirectoryItemDto> Sort(
        IEnumerable<TrainerDirectoryItemDto> rows,
        string? sortBy,
        string? sortDirection)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;

        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        var key = sortBy?.Trim().ToLowerInvariant();

        IOrderedEnumerable<TrainerDirectoryItemDto> ordered = key switch
        {
            "number" => descending
                ? rows.OrderByDescending(x => x.TrainerNumber, comparer)
                : rows.OrderBy(x => x.TrainerNumber, comparer),

            "specialty" => descending
                ? rows.OrderByDescending(x => x.Specialty ?? string.Empty, comparer)
                : rows.OrderBy(x => x.Specialty ?? string.Empty, comparer),

            "branch" => descending
                ? rows.OrderByDescending(x => x.BranchName, comparer)
                : rows.OrderBy(x => x.BranchName, comparer),

            "status" => descending
                ? rows.OrderByDescending(x => x.IsActive ? "Active" : "Inactive", comparer)
                : rows.OrderBy(x => x.IsActive ? "Active" : "Inactive", comparer),

            _ => descending
                ? rows.OrderByDescending(x => x.TrainerName, comparer)
                : rows.OrderBy(x => x.TrainerName, comparer)
        };

        return ordered.ThenBy(x => x.TrainerName, comparer);
    }

    private static bool Contains(
        string? value,
        string term)
    {
        return value?.Contains(
            term,
            StringComparison.OrdinalIgnoreCase) == true;
    }
}