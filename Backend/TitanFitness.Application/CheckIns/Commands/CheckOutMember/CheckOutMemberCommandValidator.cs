using FluentValidation;

namespace TitanFitness.Application.CheckIns.Commands.CheckOutMember;

public sealed class CheckOutMemberCommandValidator
    : AbstractValidator<CheckOutMemberCommand>
{
    public CheckOutMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0);
    }
}