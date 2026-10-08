using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Memberships.Commands.IssueGuestPass;

public sealed class IssueGuestPassCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueGuestPassCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IssueGuestPassResult> Handle(
        IssueGuestPassCommand command,
        CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetByIdAsync(
            command.MembershipId,
            cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException(
                $"Membership with id {command.MembershipId} was not found.");
        }

        var issuedOn = DateOnly.FromDateTime(DateTime.Now);

        try
        {
            var guestPass = membership.IssueGuestPass(
                issuedOn,
                command.GuestName);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new IssueGuestPassResult(
                guestPass.GuestPassId,
                membership.MembershipId,
                guestPass.IssuedOn,
                guestPass.GuestName,
                membership.GetRemainingGuestPassCount());
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(ex.Message);
        }
    }
}