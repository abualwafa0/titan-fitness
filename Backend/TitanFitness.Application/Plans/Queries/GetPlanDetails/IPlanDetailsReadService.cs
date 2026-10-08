namespace TitanFitness.Application.Plans.Queries.GetPlanDetails;

public interface IPlanDetailsReadService
{
    Task<PlanDetailsDto?> GetByIdAsync(
        int planId,
        CancellationToken cancellationToken = default);
}