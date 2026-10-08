using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Memberships.Commands.FreezeMembership;

public sealed class FreezeMembershipCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FreezeMembershipCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreezeMembershipResult> Handle(
        FreezeMembershipCommand command,
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

        var requestedOn = DateTime.Now;

        try
        {
            var freeze = membership.AddFreeze(
                command.StartDate,
                command.DurationInMonths,
                command.Reason,
                command.AdditionalNotes,
                requestedOn);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new FreezeMembershipResult(
                freeze.FreezeId,
                membership.MembershipId,
                freeze.StartDate,
                freeze.EndDate,
                freeze.DurationInMonths,
                membership.EndDate);
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