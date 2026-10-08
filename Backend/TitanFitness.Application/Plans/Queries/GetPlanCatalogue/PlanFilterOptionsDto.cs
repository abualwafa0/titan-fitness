namespace TitanFitness.Application.Plans.Queries.GetPlanCatalogue;

public sealed record PlanFilterOptionsDto(
    IReadOnlyCollection<int> Durations,
    decimal MinPrice,
    decimal MaxPrice);