using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.ClassSessions.Queries.GetBookingContext;

public sealed class GetBookingContextQueryHandler
{
    private readonly IBookingContextReadService _readService;

    public GetBookingContextQueryHandler(
        IBookingContextReadService readService)
    {
        _readService = readService;
    }

    public async Task<BookingContextDto> Handle(
        GetBookingContextQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.SessionId <= 0)
        {
            throw new ArgumentException(
                "Session id must be greater than zero.",
                nameof(query.SessionId));
        }

        var now = DateTime.Now;

        var context = await _readService.GetAsync(
            query.SessionId,
            now,
            cancellationToken);

        if (context is null)
        {
            throw new NotFoundException(
                $"Class session with id {query.SessionId} was not found.");
        }

        return context;
    }
}