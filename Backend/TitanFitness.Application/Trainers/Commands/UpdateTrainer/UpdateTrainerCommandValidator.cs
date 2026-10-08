using FluentValidation;

namespace TitanFitness.Application.Trainers.Commands.UpdateTrainer;

public sealed class UpdateTrainerCommandValidator
    : AbstractValidator<UpdateTrainerCommand>
{
    private const string PhonePattern = @"^\+?[0-9\s\-().]{7,20}$";

    public UpdateTrainerCommandValidator()
    {
        RuleFor(x => x.TrainerId)
            .GreaterThan(0);

        RuleFor(x => x.TrainerName)
            .NotEmpty()
            .WithMessage("Trainer name is required.")
            .Must(name =>
                string.IsNullOrWhiteSpace(name) ||
                (name.Trim().Length >= 2 && name.Trim().Length <= 80))
            .WithMessage("Trainer name must be between 2 and 80 characters.");

        RuleFor(x => x.Specialty)
            .MaximumLength(100)
            .WithMessage("Specialty cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Enter a valid email address.")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters.");

        RuleFor(x => x.Phone)
            .Matches(PhonePattern)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Enter a valid phone number.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch is required.");
    }
}