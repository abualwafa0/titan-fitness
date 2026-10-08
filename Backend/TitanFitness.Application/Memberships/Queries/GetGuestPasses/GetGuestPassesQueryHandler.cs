using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Memberships.Queries.GetGuestPasses;

public sealed class GetGuestPassesQueryHandler
{
    private readonly IGuestPassesReadService _readService;

    public GetGuestPassesQueryHandler(
        IGuestPassesReadService readService)
    {
        _readService = readService;
    }

    public async Task<GuestPassesDto> Handle(
        GetGuestPassesQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.MembershipId <= 0)
        {
            throw new ArgumentException(
                "Membership id must be greater than zero.",
                nameof(query.MembershipId));
        }

        var result = await _readService.GetAsync(
            query.MembershipId,
            cancellationToken);

        if (result is null)
        {
            throw new NotFoundException(
                $"Membership with id {query.MembershipId} was not found.");
        }

        return result;
    }
}