using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.CheckIns.Commands.CheckOutMember;

public sealed class CheckOutMemberCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutMemberCommandHandler(
        IMemberRepository memberRepository,
        ICheckInRepository checkInRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _checkInRepository = checkInRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckOutMemberResult> Handle(
        CheckOutMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var memberExists = await _memberRepository.ExistsAsync(
            command.MemberId,
            cancellationToken);

        if (!memberExists)
        {
            throw new NotFoundException(
                $"Member with id {command.MemberId} was not found.");
        }

        var checkIn =
            await _checkInRepository.GetOpenAdmittedCheckInForMemberAsync(
                command.MemberId,
                cancellationToken);

        if (checkIn is null)
        {
            throw new ConflictException(
                "The member does not have an open admitted check-in.");
        }

        var checkOutDateTime = DateTime.Now;

        try
        {
            checkIn.CheckOut(checkOutDateTime);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CheckOutMemberResult(
            checkIn.CheckInId,
            checkIn.MemberId,
            checkIn.BranchId,
            checkIn.CheckInDateTime,
            checkIn.CheckOutDateTime!.Value);
    }
}