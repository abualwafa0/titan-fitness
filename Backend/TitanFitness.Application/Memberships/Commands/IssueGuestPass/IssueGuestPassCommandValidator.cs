using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.IssueGuestPass;

public sealed class IssueGuestPassCommandValidator
    : AbstractValidator<IssueGuestPassCommand>
{
    public IssueGuestPassCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);

        RuleFor(x => x.GuestName)
            .MaximumLength(100);
    }
}