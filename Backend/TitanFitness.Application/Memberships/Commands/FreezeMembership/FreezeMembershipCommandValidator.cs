using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.FreezeMembership;

public sealed class FreezeMembershipCommandValidator
    : AbstractValidator<FreezeMembershipCommand>
{
    public FreezeMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.DurationInMonths)
            .Must(x => x is 1 or 2 or 3)
            .WithMessage(
                "Freeze duration must be 1, 2, or 3 months.");

        RuleFor(x => x.Reason)
            .IsInEnum();

        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(500);
    }
}