using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Bookings.Commands.BookSession;

public sealed class BookSessionCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IClassSessionRepository _classSessionRepository;
    private readonly BookingEligibilityService _bookingEligibilityService;
    private readonly IUnitOfWork _unitOfWork;

    public BookSessionCommandHandler(
        IMemberRepository memberRepository,
        IMembershipRepository membershipRepository,
        IClassSessionRepository classSessionRepository,
        BookingEligibilityService bookingEligibilityService,
        IUnitOfWork unitOfWork)
    {
        _memberRepository =
            memberRepository;

        _membershipRepository =
            membershipRepository;

        _classSessionRepository =
            classSessionRepository;

        _bookingEligibilityService =
            bookingEligibilityService;

        _unitOfWork =
            unitOfWork;
    }

    public async Task<BookSessionResult> Handle(
        BookSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var member =
            await _memberRepository.GetByIdAsync(
                command.MemberId,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException(
                $"Member with id {command.MemberId} was not found.");
        }

        var classSession =
            await _classSessionRepository.GetByIdAsync(
                command.SessionId,
                cancellationToken);

        if (classSession is null)
        {
            throw new NotFoundException(
                $"Class session with id {command.SessionId} was not found.");
        }

        var bookedOn =
            DateTime.Now;

        var membership =
            await _membershipRepository
                .GetCurrentForMemberAsync(
                    command.MemberId,
                    classSession.SessionDate,
                    cancellationToken);

        try
        {
            await _bookingEligibilityService
                .EnsureCanBookAsync(
                    member,
                    membership,
                    classSession.SessionDate,
                    classSession.BranchId,
                    classSession.GetTimeRange(),
                    classSession.SessionId,
                    cancellationToken);

            var booking =
                classSession.AddBooking(
                    command.MemberId,
                    bookedOn,
                    command.NotesForTrainer);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new BookSessionResult(
                booking.BookingId,
                classSession.SessionId,
                booking.MemberId,
                booking.BookedOn,
                booking.Status,
                booking.WaitlistPosition);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(
                ex.Message);
        }
    }
}