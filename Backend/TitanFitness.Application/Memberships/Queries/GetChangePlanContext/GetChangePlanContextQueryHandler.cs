using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Memberships.Queries.GetChangePlanContext;

public sealed class GetChangePlanContextQueryHandler
{
    private readonly IChangePlanContextReadService _readService;

    public GetChangePlanContextQueryHandler(
        IChangePlanContextReadService readService)
    {
        _readService = readService;
    }

    public async Task<ChangePlanContextDto> Handle(
        GetChangePlanContextQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.MembershipId <= 0)
        {
            throw new ArgumentException(
                "Membership id must be greater than zero.",
                nameof(query.MembershipId));
        }

        var today = DateOnly.FromDateTime(DateTime.Now);

        var context = await _readService.GetAsync(
            query.MembershipId,
            today,
            cancellationToken);

        if (context is null)
        {
            throw new NotFoundException(
                $"Membership with id {query.MembershipId} was not found.");
        }

        return context;
    }
}