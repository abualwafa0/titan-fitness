using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.Bookings.Commands.BookSession;

public sealed record BookSessionCommand(
    int SessionId,
    int MemberId,
    string? NotesForTrainer);

public sealed record BookSessionResult(
    int BookingId,
    int SessionId,
    int MemberId,
    DateTime BookedOn,
    BookingStatus Status,
    int? WaitlistPosition);