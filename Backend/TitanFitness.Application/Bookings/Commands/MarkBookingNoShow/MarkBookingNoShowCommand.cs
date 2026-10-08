namespace TitanFitness.Application.Bookings.Commands.MarkBookingNoShow;

public sealed record MarkBookingNoShowCommand(
    int SessionId,
    int BookingId);