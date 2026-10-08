using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Memberships.Commands.CancelMembership;

public sealed class CancelMembershipCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMembershipCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelMembershipCommand command,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _membershipRepository.GetByIdAsync(
                command.MembershipId,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException(
                $"Membership with id {command.MembershipId} was not found.");
        }

        var today =
            DateOnly.FromDateTime(DateTime.Now);

        try
        {
            membership.Cancel(today);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}