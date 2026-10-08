using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Models;
using TitanFitness.Application.Plans.Queries.GetPlanCatalogue;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Plans;

public sealed class PlanCatalogueReadService
    : IPlanCatalogueReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public PlanCatalogueReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PlanCatalogueItemDto>> GetAsync(
        string? search,
        int? branchId,
        bool? isPublished,
        IReadOnlyCollection<int>? durations,
        AccessScope? accessScope,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Plans
                .AsNoTracking()
                .AsQueryable();

        if (branchId.HasValue)
        {
            var selectedBranchId = branchId.Value;

            query = query.Where(plan =>
                !plan.AvailableBranches.Any() ||
                plan.AvailableBranches.Any(
                    branch => branch.BranchId == selectedBranchId));
        }

        if (isPublished.HasValue)
        {
            query = query.Where(x =>
                x.IsPublished == isPublished.Value);
        }

        if (durations is not null && durations.Count > 0)
        {
            var durationList = durations.Distinct().ToList();

            query = query.Where(x =>
                durationList.Contains(x.DurationInMonths));
        }

        if (accessScope.HasValue)
        {
            var scope = accessScope.Value;

            query = query.Where(x => x.AccessScope == scope);
        }

        if (minPrice.HasValue)
        {
            var min = minPrice.Value;

            query = query.Where(x => x.Price >= min);
        }

        if (maxPrice.HasValue)
        {
            var max = maxPrice.Value;

            query = query.Where(x => x.Price <= max);
        }

        var rows = await query
            .Select(x => new PlanCatalogueItemDto(
                x.PlanId,
                x.PlanName,
                x.Price,
                x.DurationInMonths,
                x.MaximumFreezeDays,
                x.MaximumNumberOfFreezes,
                x.GuestPassQuota,
                x.AccessScope,
                x.IsPublished))
            .ToListAsync(cancellationToken);

        IEnumerable<PlanCatalogueItemDto> filteredRows = rows;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var priceTerm = term.TrimStart('$');

            filteredRows = filteredRows.Where(x =>
                Contains(x.PlanName, term) ||
                Contains(PriceText(x), priceTerm) ||
                Contains(DurationText(x), term) ||
                Contains(FreezeText(x), term) ||
                Contains(x.GuestPassQuota.ToString(), term) ||
                Contains(AccessText(x), term) ||
                Contains(StatusText(x), term));
        }

        var filteredList = Sort(filteredRows, sortBy, sortDirection)
            .ToList();

        var totalCount = filteredList.Count;

        var items = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PlanCatalogueItemDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<PlanFilterOptionsDto> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var durations = await _dbContext.Plans
            .AsNoTracking()
            .Select(x => x.DurationInMonths)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var prices = await _dbContext.Plans
            .AsNoTracking()
            .Select(x => x.Price)
            .ToListAsync(cancellationToken);

        var minPrice = prices.Count == 0 ? 0m : prices.Min();
        var maxPrice = prices.Count == 0 ? 0m : prices.Max();

        return new PlanFilterOptionsDto(
            durations,
            minPrice,
            maxPrice);
    }

    private static IOrderedEnumerable<PlanCatalogueItemDto> Sort(
        IEnumerable<PlanCatalogueItemDto> rows,
        string? sortBy,
        string? sortDirection)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;

        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        var key = sortBy?.Trim().ToLowerInvariant();

        IOrderedEnumerable<PlanCatalogueItemDto> ordered = key switch
        {
            "price" => descending
                ? rows.OrderByDescending(x => x.Price)
                : rows.OrderBy(x => x.Price),

            "duration" => descending
                ? rows.OrderByDescending(x => x.DurationInMonths)
                : rows.OrderBy(x => x.DurationInMonths),

            "guestpasses" => descending
                ? rows.OrderByDescending(x => x.GuestPassQuota)
                : rows.OrderBy(x => x.GuestPassQuota),

            "access" => descending
                ? rows.OrderByDescending(x => AccessText(x), comparer)
                : rows.OrderBy(x => AccessText(x), comparer),

            "status" => descending
                ? rows.OrderByDescending(x => StatusText(x), comparer)
                : rows.OrderBy(x => StatusText(x), comparer),

            _ => descending
                ? rows.OrderByDescending(x => x.PlanName, comparer)
                : rows.OrderBy(x => x.PlanName, comparer)
        };

        return ordered.ThenBy(x => x.PlanName, comparer);
    }

    private static string PriceText(PlanCatalogueItemDto plan)
    {
        return plan.Price.ToString(
            "0.00",
            CultureInfo.InvariantCulture);
    }

    private static string DurationText(PlanCatalogueItemDto plan)
    {
        return plan.DurationInMonths == 1
            ? "1 month"
            : $"{plan.DurationInMonths} months";
    }

    private static string FreezeText(PlanCatalogueItemDto plan)
    {
        if (plan.MaximumFreezeDays == 0)
        {
            return "None";
        }

        var freezes = plan.MaximumNumberOfFreezes == 1
            ? "freeze"
            : "freezes";

        return $"{plan.MaximumFreezeDays} days / " +
               $"{plan.MaximumNumberOfFreezes} {freezes}";
    }

    private static string AccessText(PlanCatalogueItemDto plan)
    {
        return plan.AccessScope == AccessScope.AllBranches
            ? "All branches"
            : "Home branch only";
    }

    private static string StatusText(PlanCatalogueItemDto plan)
    {
        return plan.IsPublished
            ? "Published"
            : "Retired";
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