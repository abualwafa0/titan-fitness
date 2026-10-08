using FluentValidation;

namespace TitanFitness.Application.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandValidator
    : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty()
            .WithMessage("Plan name is required.")
            .Must(name =>
                string.IsNullOrWhiteSpace(name) ||
                (name.Trim().Length >= 2 && name.Trim().Length <= 60))
            .WithMessage("Plan name must be between 2 and 60 characters.");

        RuleFor(x => x.Price)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.")
            .Must(HaveMaximumTwoDecimalPlaces)
            .WithMessage("Price cannot have more than two decimal places.");

        RuleFor(x => x.DurationInMonths)
            .InclusiveBetween(1, 36)
            .WithMessage("Duration must be a whole number between 1 and 36.");

        RuleFor(x => x.MaximumFreezeDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum freeze days cannot be negative.");

        RuleFor(x => x.MaximumNumberOfFreezes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum number of freezes cannot be negative.");

        RuleFor(x => x.MaximumNumberOfFreezes)
            .Equal(0)
            .When(x => x.MaximumFreezeDays == 0)
            .WithMessage(
                "Maximum number of freezes must be 0 when maximum freeze days is 0.");

        RuleFor(x => x.GuestPassQuota)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Guest pass quota cannot be negative.");

        RuleFor(x => x.AccessScope)
            .IsInEnum();
    }

    private static bool HaveMaximumTwoDecimalPlaces(
        decimal price)
    {
        return decimal.Round(price, 2) == price;
    }
}