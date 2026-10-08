using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Plans.Queries.GetPlanDetails;

public sealed class GetPlanDetailsQueryHandler
{
    private readonly IPlanDetailsReadService _readService;

    public GetPlanDetailsQueryHandler(
        IPlanDetailsReadService readService)
    {
        _readService = readService;
    }

    public async Task<PlanDetailsDto> Handle(
        GetPlanDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.PlanId <= 0)
        {
            throw new ArgumentException(
                "Plan id must be greater than zero.");
        }

        var plan = await _readService.GetByIdAsync(
            query.PlanId,
            cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(
                $"Plan with id {query.PlanId} was not found.");
        }

        return plan;
    }
}