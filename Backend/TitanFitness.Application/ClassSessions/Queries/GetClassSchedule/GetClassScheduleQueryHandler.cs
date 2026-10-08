using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

public sealed class GetClassScheduleQueryHandler
{
    private readonly IClassScheduleReadService _readService;
    private readonly IBranchRepository _branchRepository;

    public GetClassScheduleQueryHandler(
        IClassScheduleReadService readService,
        IBranchRepository branchRepository)
    {
        _readService = readService;
        _branchRepository = branchRepository;
    }

    public async Task<ClassScheduleDto> Handle(
        GetClassScheduleQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Date == default)
        {
            throw new ArgumentException(
                "Date is required.",
                nameof(query.Date));
        }

        if (query.ToDate.HasValue)
        {
            if (query.ToDate.Value < query.Date)
            {
                throw new ArgumentException(
                    "The end date cannot be before the start date.",
                    nameof(query.ToDate));
            }

            if (query.ToDate.Value.DayNumber -
                query.Date.DayNumber > 31)
            {
                throw new ArgumentException(
                    "The date range cannot exceed 31 days.",
                    nameof(query.ToDate));
            }
        }

        if (query.BranchId.HasValue)
        {
            if (query.BranchId.Value <= 0)
            {
                throw new ArgumentException(
                    "Branch id must be greater than zero.",
                    nameof(query.BranchId));
            }

            var branchExists =
                await _branchRepository.ExistsAsync(
                    query.BranchId.Value,
                    cancellationToken);

            if (!branchExists)
            {
                throw new NotFoundException(
                    $"Branch with id {query.BranchId.Value} was not found.");
            }
        }

        var now = DateTime.Now;

        return await _readService.GetAsync(
            query.Date,
            query.ToDate,
            now,
            query.BranchId,
            cancellationToken);
    }
}