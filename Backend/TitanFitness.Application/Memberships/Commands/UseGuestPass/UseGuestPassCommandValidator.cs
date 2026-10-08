using FluentValidation;

namespace TitanFitness.Application.Memberships.Commands.UseGuestPass;

public sealed class UseGuestPassCommandValidator
    : AbstractValidator<UseGuestPassCommand>
{
    public UseGuestPassCommandValidator()
    {
        RuleFor(x => x.MembershipId)
            .GreaterThan(0);

        RuleFor(x => x.GuestPassId)
            .GreaterThan(0);
    }
}