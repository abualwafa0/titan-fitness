using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.API.Contracts.CheckIns;
using TitanFitness.Application.CheckIns.Commands.CheckInMember;
using TitanFitness.Application.CheckIns.Commands.CheckOutMember;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/check-ins")]
public sealed class CheckInsController : ControllerBase
{
    private readonly CheckInMemberCommandHandler _checkInHandler;
    private readonly CheckOutMemberCommandHandler _checkOutHandler;
    private readonly IValidator<CheckInMemberCommand> _checkInValidator;
    private readonly IValidator<CheckOutMemberCommand> _checkOutValidator;

    public CheckInsController(
        CheckInMemberCommandHandler checkInHandler,
        CheckOutMemberCommandHandler checkOutHandler,
        IValidator<CheckInMemberCommand> checkInValidator,
        IValidator<CheckOutMemberCommand> checkOutValidator)
    {
        _checkInHandler = checkInHandler;
        _checkOutHandler = checkOutHandler;
        _checkInValidator = checkInValidator;
        _checkOutValidator = checkOutValidator;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> PreviewCheckIn(
        [FromBody] CheckInMemberRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CheckInMemberCommand(
            request.MemberId,
            request.BranchId,
            request.CheckInDateTime,
            request.Notes);

        await _checkInValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result =
            await _checkInHandler.Preview(
                command,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInMemberRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CheckInMemberCommand(
            request.MemberId,
            request.BranchId,
            request.CheckInDateTime,
            request.Notes);

        await _checkInValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result =
            await _checkInHandler.Handle(
                command,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost("members/{memberId:int}/check-out")]
    public async Task<IActionResult> CheckOut(
        int memberId,
        CancellationToken cancellationToken)
    {
        var command =
            new CheckOutMemberCommand(
                memberId);

        await _checkOutValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result =
            await _checkOutHandler.Handle(
                command,
                cancellationToken);

        return Ok(result);
    }
}