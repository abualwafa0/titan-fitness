namespace TitanFitness.Application.ClassSessions.Queries.GetBookingContext;

public interface IBookingContextReadService
{
    Task<BookingContextDto?> GetAsync(
        int sessionId,
        DateTime now,
        CancellationToken cancellationToken = default);
}