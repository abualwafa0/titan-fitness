using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Memberships.Queries.GetFreezeContext;

public sealed class GetFreezeContextQueryHandler
{
    private readonly IFreezeContextReadService _readService;

    public GetFreezeContextQueryHandler(
        IFreezeContextReadService readService)
    {
        _readService = readService;
    }

    public async Task<FreezeContextDto> Handle(
        GetFreezeContextQuery query,
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