using FluentValidation;

namespace TitanFitness.Application.ClassSessions.Commands.CancelClassSession;

public sealed class CancelClassSessionCommandValidator
    : AbstractValidator<CancelClassSessionCommand>
{
    public CancelClassSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0);
    }
}