using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.CancelMembership;

public sealed class CancelMembershipCommandValidator
    : AbstractValidator<CancelMembershipCommand>
{
    public CancelMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);
    }
}