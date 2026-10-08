using FluentValidation;

namespace TitanFitness.Application.Bookings.Commands.MarkBookingAttended;

public sealed class MarkBookingAttendedCommandValidator
    : AbstractValidator<MarkBookingAttendedCommand>
{
    public MarkBookingAttendedCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0);

        RuleFor(x => x.BookingId)
            .GreaterThan(0);
    }
}