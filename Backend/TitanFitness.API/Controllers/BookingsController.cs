using FluentValidation;
using Microsoft.AspNetCore.Mvc;

using TitanFitness.API.Contracts.Bookings;

using TitanFitness.Application.Bookings.Commands.BookSession;
using TitanFitness.Application.Bookings.Commands.CancelBooking;
using TitanFitness.Application.Bookings.Commands.MarkBookingAttended;
using TitanFitness.Application.Bookings.Commands.MarkBookingNoShow;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/class-sessions/{sessionId:int}/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly BookSessionCommandHandler _bookHandler;
    private readonly CancelBookingCommandHandler _cancelHandler;
    private readonly MarkBookingAttendedCommandHandler _attendedHandler;
    private readonly MarkBookingNoShowCommandHandler _noShowHandler;

    private readonly IValidator<BookSessionCommand> _bookValidator;
    private readonly IValidator<CancelBookingCommand> _cancelValidator;
    private readonly IValidator<MarkBookingAttendedCommand> _attendedValidator;
    private readonly IValidator<MarkBookingNoShowCommand> _noShowValidator;

    public BookingsController(
        BookSessionCommandHandler bookHandler,
        CancelBookingCommandHandler cancelHandler,
        MarkBookingAttendedCommandHandler attendedHandler,
        MarkBookingNoShowCommandHandler noShowHandler,
        IValidator<BookSessionCommand> bookValidator,
        IValidator<CancelBookingCommand> cancelValidator,
        IValidator<MarkBookingAttendedCommand> attendedValidator,
        IValidator<MarkBookingNoShowCommand> noShowValidator)
    {
        _bookHandler = bookHandler;
        _cancelHandler = cancelHandler;
        _attendedHandler = attendedHandler;
        _noShowHandler = noShowHandler;
        _bookValidator = bookValidator;
        _cancelValidator = cancelValidator;
        _attendedValidator = attendedValidator;
        _noShowValidator = noShowValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Book(
        int sessionId,
        [FromBody] BookSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BookSessionCommand(
            sessionId,
            request.MemberId,
            request.NotesForTrainer);

        await _bookValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _bookHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int sessionId,
        int bookingId,
        CancellationToken cancellationToken)
    {
        var command = new CancelBookingCommand(
            sessionId,
            bookingId);

        await _cancelValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _cancelHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{bookingId:int}/attended")]
    public async Task<IActionResult> MarkAttended(
        int sessionId,
        int bookingId,
        CancellationToken cancellationToken)
    {
        var command = new MarkBookingAttendedCommand(
            sessionId,
            bookingId);

        await _attendedValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _attendedHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{bookingId:int}/no-show")]
    public async Task<IActionResult> MarkNoShow(
        int sessionId,
        int bookingId,
        CancellationToken cancellationToken)
    {
        var command = new MarkBookingNoShowCommand(
            sessionId,
            bookingId);

        await _noShowValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _noShowHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }
}