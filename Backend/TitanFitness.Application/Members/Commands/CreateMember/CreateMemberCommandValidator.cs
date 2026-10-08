using System.Text.RegularExpressions;
using FluentValidation;

namespace TitanFitness.Application.Members.Commands.CreateMember;

public sealed class CreateMemberCommandValidator
    : AbstractValidator<CreateMemberCommand>
{
    private static readonly Regex NamePattern =
        new(@"^[\p{L}\s'’\-]+$", RegexOptions.Compiled);

    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.MembershipNumber)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.MembershipNumber));

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Member name is required.")
            .Must(name =>
                string.IsNullOrWhiteSpace(name) ||
                (name.Trim().Length >= 2 && name.Trim().Length <= 80))
            .WithMessage("Member name must be between 2 and 80 characters.")
            .Must(name =>
                string.IsNullOrWhiteSpace(name) ||
                NamePattern.IsMatch(name.Trim()))
            .WithMessage(
                "Member name can contain only letters, spaces, hyphens and apostrophes.");

        RuleFor(x => x.Email)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.JoinedDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.HomeBranchId)
            .GreaterThan(0)
            .WithMessage("Branch is required.");

        RuleFor(x => x.PhotoFileName)
            .NotEmpty()
            .When(x => x.PhotoStream is not null);

        RuleFor(x => x.PhotoStream)
            .NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoFileName));
    }
}