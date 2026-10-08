using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.RenewMembership;

public sealed class RenewMembershipCommandValidator
    : AbstractValidator<RenewMembershipCommand>
{
    public RenewMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);
    }
}