using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Members.Queries.GetMemberProfile;

public sealed class GetMemberProfileQueryHandler
{
    private readonly IMemberProfileReadService _readService;

    public GetMemberProfileQueryHandler(
        IMemberProfileReadService readService)
    {
        _readService = readService;
    }

    public async Task<MemberProfileDto> Handle(
        GetMemberProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.MemberId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.MemberId),
                "Member ID must be greater than zero.");
        }

        var today = DateOnly.FromDateTime(DateTime.Now);

        var member = await _readService.GetAsync(
            query.MemberId,
            today,
            cancellationToken);

        if (member is null)
        {
            throw new NotFoundException(
                $"Member with id {query.MemberId} was not found.");
        }

        return member;
    }
}