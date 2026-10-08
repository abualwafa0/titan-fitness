using FluentValidation;

namespace TitanFitness.Application.CheckIns.Commands.CheckInMember;

public sealed class CheckInMemberCommandValidator
    : AbstractValidator<CheckInMemberCommand>
{
    public CheckInMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .WithMessage("Member is required.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch is required.");

        RuleFor(x => x.CheckInDateTime)
            .Must(value =>
                !value.HasValue ||
                value.Value <= DateTime.Now.AddMinutes(1))
            .WithMessage("Check-in time cannot be in the future.");

        RuleFor(x => x.CheckInDateTime)
            .Must(value =>
                !value.HasValue ||
                value.Value.Date >= DateTime.Today.AddDays(-7))
            .WithMessage("Check-in date cannot be older than 7 days.");

        RuleFor(x => x.Notes)
            .MaximumLength(250)
            .WithMessage("Notes cannot exceed 250 characters.");
    }
}