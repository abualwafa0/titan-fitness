using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Memberships.Commands.UseGuestPass;

public sealed class UseGuestPassCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UseGuestPassCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UseGuestPassResult> Handle(
        UseGuestPassCommand command,
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

        var usedOn = DateOnly.FromDateTime(DateTime.Now);

        try
        {
            membership.UseGuestPass(
                command.GuestPassId,
                usedOn);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new UseGuestPassResult(
            command.GuestPassId,
            membership.MembershipId,
            usedOn);
    }
}