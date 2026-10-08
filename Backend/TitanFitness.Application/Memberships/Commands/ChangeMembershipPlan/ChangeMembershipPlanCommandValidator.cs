using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;

public sealed class ChangeMembershipPlanCommandValidator
    : AbstractValidator<ChangeMembershipPlanCommand>
{
    public ChangeMembershipPlanCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);

        RuleFor(x => x.NewPlanId)
            .GreaterThan(0);

        RuleFor(x => x.Timing)
            .IsInEnum();
    }
}