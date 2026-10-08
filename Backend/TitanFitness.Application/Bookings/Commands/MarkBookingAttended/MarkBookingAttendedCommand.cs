namespace TitanFitness.Application.Bookings.Commands.MarkBookingAttended;

public sealed record MarkBookingAttendedCommand(
    int SessionId,
    int BookingId);