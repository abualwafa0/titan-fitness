using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.PurchaseMembership;

public sealed class PurchaseMembershipCommandValidator
    : AbstractValidator<PurchaseMembershipCommand>
{
    public PurchaseMembershipCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0);

        RuleFor(x => x.PlanId)
            .GreaterThan(0);

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly));
    }
}