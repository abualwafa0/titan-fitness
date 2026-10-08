using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.API.Contracts.ClassSessions;
using TitanFitness.Application.ClassSessions.Commands.CancelClassSession;
using TitanFitness.Application.ClassSessions.Commands.ScheduleClass;
using TitanFitness.Application.ClassSessions.Commands.UpdateClass;
using TitanFitness.Application.ClassSessions.Queries.GetBookingContext;
using TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/class-sessions")]
public sealed class ClassSessionsController : ControllerBase
{
    private readonly ScheduleClassCommandHandler _scheduleHandler;
    private readonly UpdateClassCommandHandler _updateHandler;
    private readonly CancelClassSessionCommandHandler _cancelHandler;
    private readonly GetClassScheduleQueryHandler _scheduleQueryHandler;
    private readonly GetBookingContextQueryHandler _bookingContextHandler;
    private readonly IValidator<ScheduleClassCommand> _scheduleValidator;
    private readonly IValidator<UpdateClassCommand> _updateValidator;
    private readonly IValidator<CancelClassSessionCommand> _cancelValidator;

    public ClassSessionsController(
        ScheduleClassCommandHandler scheduleHandler,
        UpdateClassCommandHandler updateHandler,
        CancelClassSessionCommandHandler cancelHandler,
        GetClassScheduleQueryHandler scheduleQueryHandler,
        GetBookingContextQueryHandler bookingContextHandler,
        IValidator<ScheduleClassCommand> scheduleValidator,
        IValidator<UpdateClassCommand> updateValidator,
        IValidator<CancelClassSessionCommand> cancelValidator)
    {
        _scheduleHandler = scheduleHandler;
        _updateHandler = updateHandler;
        _cancelHandler = cancelHandler;
        _scheduleQueryHandler = scheduleQueryHandler;
        _bookingContextHandler = bookingContextHandler;
        _scheduleValidator = scheduleValidator;
        _updateValidator = updateValidator;
        _cancelValidator = cancelValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateOnly date,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? branchId,
        CancellationToken cancellationToken)
    {
        var query = new GetClassScheduleQuery(
            date,
            branchId,
            toDate);

        var result = await _scheduleQueryHandler.Handle(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{sessionId:int}/booking-context")]
    public async Task<IActionResult> GetBookingContext(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _bookingContextHandler.Handle(
            new GetBookingContextQuery(sessionId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Schedule(
        [FromBody] ScheduleClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleClassCommand(
            request.ClassName,
            request.BranchId,
            request.StudioId,
            request.TrainerId,
            request.SessionDate,
            request.StartTime,
            request.DurationInMinutes,
            request.CapacityLimit,
            request.Description);

        await _scheduleValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _scheduleHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{sessionId:int}")]
    public async Task<IActionResult> Update(
        int sessionId,
        [FromBody] UpdateClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassCommand(
            sessionId,
            request.ClassName,
            request.BranchId,
            request.StudioId,
            request.TrainerId,
            request.SessionDate,
            request.StartTime,
            request.DurationInMinutes,
            request.CapacityLimit,
            request.Description);

        await _updateValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _updateHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{sessionId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var command = new CancelClassSessionCommand(sessionId);

        await _cancelValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _cancelHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }
}