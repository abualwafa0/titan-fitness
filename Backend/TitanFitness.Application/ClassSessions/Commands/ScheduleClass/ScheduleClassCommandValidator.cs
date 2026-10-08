using FluentValidation;

namespace TitanFitness.Application.ClassSessions.Commands.ScheduleClass;

public sealed class ScheduleClassCommandValidator
    : AbstractValidator<ScheduleClassCommand>
{
    public ScheduleClassCommandValidator()
    {
        RuleFor(x => x.ClassName)
            .NotEmpty()
            .WithMessage("Class name is required.")
            .Must(name =>
                string.IsNullOrWhiteSpace(name) ||
                (name.Trim().Length >= 3 && name.Trim().Length <= 80))
            .WithMessage("Class name must be between 3 and 80 characters.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch is required.");

        RuleFor(x => x.StudioId)
            .GreaterThan(0)
            .When(x => x.StudioId.HasValue);

        RuleFor(x => x.TrainerId)
            .GreaterThan(0)
            .When(x => x.TrainerId.HasValue);

        RuleFor(x => x.SessionDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Date is required.");

        RuleFor(x => x.SessionDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.SessionDate != default)
            .WithMessage("Date cannot be in the past.");

        RuleFor(x => x.StartTime)
            .Must((command, time) =>
                command.SessionDate == default ||
                command.SessionDate > DateOnly.FromDateTime(DateTime.Today) ||
                time > TimeOnly.FromDateTime(DateTime.Now))
            .WithMessage("Start time must be later than now.");

        RuleFor(x => x.DurationInMinutes)
            .Must(x => x is 30 or 45 or 60)
            .WithMessage("Session duration must be 30, 45, or 60 minutes.");

        RuleFor(x => x.CapacityLimit)
            .InclusiveBetween(1, 100)
            .When(x => x.CapacityLimit.HasValue)
            .WithMessage("Capacity must be a whole number between 1 and 100.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}