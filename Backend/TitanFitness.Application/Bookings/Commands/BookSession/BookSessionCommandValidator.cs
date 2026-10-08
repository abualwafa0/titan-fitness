using FluentValidation;

namespace TitanFitness.Application.Bookings.Commands.BookSession;

public sealed class BookSessionCommandValidator
    : AbstractValidator<BookSessionCommand>
{
    public BookSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0);

        RuleFor(x => x.MemberId)
            .GreaterThan(0);

        RuleFor(x => x.NotesForTrainer)
            .MaximumLength(500);
    }
}