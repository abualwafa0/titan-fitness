using FluentValidation;

namespace TitanFitness.Application.Bookings.Commands.MarkBookingNoShow;

public sealed class MarkBookingNoShowCommandValidator
    : AbstractValidator<MarkBookingNoShowCommand>
{
    public MarkBookingNoShowCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0);

        RuleFor(x => x.BookingId)
            .GreaterThan(0);
    }
}