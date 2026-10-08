using FluentValidation;

namespace TitanFitness.Application.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandValidator
    : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0);

        RuleFor(x => x.BookingId)
            .GreaterThan(0);
    }
}