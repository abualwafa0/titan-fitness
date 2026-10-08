using FluentValidation;
using Microsoft.AspNetCore.Mvc;

using TitanFitness.API.Contracts.Memberships;

using TitanFitness.Application.Memberships.Commands.CancelMembership;
using TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;
using TitanFitness.Application.Memberships.Commands.FreezeMembership;
using TitanFitness.Application.Memberships.Commands.IssueGuestPass;
using TitanFitness.Application.Memberships.Commands.PurchaseMembership;
using TitanFitness.Application.Memberships.Commands.RenewMembership;
using TitanFitness.Application.Memberships.Commands.UseGuestPass;

using TitanFitness.Application.Memberships.Queries.GetChangePlanContext;
using TitanFitness.Application.Memberships.Queries.GetFreezeContext;
using TitanFitness.Application.Memberships.Queries.GetGuestPasses;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/memberships")]
public sealed class MembershipsController : ControllerBase
{
    private readonly PurchaseMembershipCommandHandler _purchaseHandler;
    private readonly RenewMembershipCommandHandler _renewHandler;
    private readonly ChangeMembershipPlanCommandHandler _changePlanHandler;
    private readonly CancelMembershipCommandHandler _cancelHandler;
    private readonly FreezeMembershipCommandHandler _freezeHandler;
    private readonly IssueGuestPassCommandHandler _issueGuestPassHandler;
    private readonly UseGuestPassCommandHandler _useGuestPassHandler;

    private readonly GetChangePlanContextQueryHandler _changePlanContextHandler;
    private readonly GetFreezeContextQueryHandler _freezeContextHandler;
    private readonly GetGuestPassesQueryHandler _guestPassesHandler;

    private readonly IValidator<PurchaseMembershipCommand> _purchaseValidator;
    private readonly IValidator<RenewMembershipCommand> _renewValidator;
    private readonly IValidator<ChangeMembershipPlanCommand> _changePlanValidator;
    private readonly IValidator<CancelMembershipCommand> _cancelValidator;
    private readonly IValidator<FreezeMembershipCommand> _freezeValidator;
    private readonly IValidator<IssueGuestPassCommand> _issueGuestPassValidator;
    private readonly IValidator<UseGuestPassCommand> _useGuestPassValidator;

    public MembershipsController(
        PurchaseMembershipCommandHandler purchaseHandler,
        RenewMembershipCommandHandler renewHandler,
        ChangeMembershipPlanCommandHandler changePlanHandler,
        CancelMembershipCommandHandler cancelHandler,
        FreezeMembershipCommandHandler freezeHandler,
        IssueGuestPassCommandHandler issueGuestPassHandler,
        UseGuestPassCommandHandler useGuestPassHandler,
        GetChangePlanContextQueryHandler changePlanContextHandler,
        GetFreezeContextQueryHandler freezeContextHandler,
        GetGuestPassesQueryHandler guestPassesHandler,
        IValidator<PurchaseMembershipCommand> purchaseValidator,
        IValidator<RenewMembershipCommand> renewValidator,
        IValidator<ChangeMembershipPlanCommand> changePlanValidator,
        IValidator<CancelMembershipCommand> cancelValidator,
        IValidator<FreezeMembershipCommand> freezeValidator,
        IValidator<IssueGuestPassCommand> issueGuestPassValidator,
        IValidator<UseGuestPassCommand> useGuestPassValidator)
    {
        _purchaseHandler = purchaseHandler;
        _renewHandler = renewHandler;
        _changePlanHandler = changePlanHandler;
        _cancelHandler = cancelHandler;
        _freezeHandler = freezeHandler;
        _issueGuestPassHandler = issueGuestPassHandler;
        _useGuestPassHandler = useGuestPassHandler;

        _changePlanContextHandler = changePlanContextHandler;
        _freezeContextHandler = freezeContextHandler;
        _guestPassesHandler = guestPassesHandler;

        _purchaseValidator = purchaseValidator;
        _renewValidator = renewValidator;
        _changePlanValidator = changePlanValidator;
        _cancelValidator = cancelValidator;
        _freezeValidator = freezeValidator;
        _issueGuestPassValidator = issueGuestPassValidator;
        _useGuestPassValidator = useGuestPassValidator;
    }

    [HttpPost("members/{memberId:int}")]
    public async Task<IActionResult> Purchase(
        int memberId,
        [FromBody] PurchaseMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PurchaseMembershipCommand(
            memberId,
            request.PlanId,
            request.StartDate);

        await _purchaseValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _purchaseHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/renew")]
    public async Task<IActionResult> Renew(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var command = new RenewMembershipCommand(
            membershipId);

        await _renewValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _renewHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{membershipId:int}/change-plan-context")]
    public async Task<IActionResult> GetChangePlanContext(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var result = await _changePlanContextHandler.Handle(
            new GetChangePlanContextQuery(membershipId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/change-plan")]
    public async Task<IActionResult> ChangePlan(
        int membershipId,
        [FromBody] ChangeMembershipPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeMembershipPlanCommand(
            membershipId,
            request.NewPlanId,
            request.Timing);

        await _changePlanValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _changePlanHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var command = new CancelMembershipCommand(
            membershipId);

        await _cancelValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _cancelHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{membershipId:int}/freeze-context")]
    public async Task<IActionResult> GetFreezeContext(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var result = await _freezeContextHandler.Handle(
            new GetFreezeContextQuery(membershipId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/freeze")]
    public async Task<IActionResult> Freeze(
        int membershipId,
        [FromBody] FreezeMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FreezeMembershipCommand(
            membershipId,
            request.StartDate,
            request.DurationInMonths,
            request.Reason,
            request.AdditionalNotes);

        await _freezeValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _freezeHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{membershipId:int}/guest-passes")]
    public async Task<IActionResult> GetGuestPasses(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var result = await _guestPassesHandler.Handle(
            new GetGuestPassesQuery(membershipId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/guest-passes")]
    public async Task<IActionResult> IssueGuestPass(
        int membershipId,
        [FromBody] IssueGuestPassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new IssueGuestPassCommand(
            membershipId,
            request.GuestName);

        await _issueGuestPassValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _issueGuestPassHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{membershipId:int}/guest-passes/use")]
    public async Task<IActionResult> UseGuestPass(
        int membershipId,
        [FromBody] UseGuestPassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UseGuestPassCommand(
            membershipId,
            request.GuestPassId);

        await _useGuestPassValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _useGuestPassHandler.Handle(
            command,
            cancellationToken);

        return Ok(result);
    }
}