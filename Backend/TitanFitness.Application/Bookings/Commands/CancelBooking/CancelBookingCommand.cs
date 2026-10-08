namespace TitanFitness.Application.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(
    int SessionId,
    int BookingId);