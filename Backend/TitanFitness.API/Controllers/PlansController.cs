using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.API.Contracts.Plans;
using TitanFitness.Application.Plans.Commands.CreatePlan;
using TitanFitness.Application.Plans.Commands.UpdatePlan;
using TitanFitness.Application.Plans.Queries.GetPlanCatalogue;
using TitanFitness.Application.Plans.Queries.GetPlanDetails;
using TitanFitness.Domain.Enums;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/plans")]
public sealed class PlansController
    : ControllerBase
{
    private readonly CreatePlanCommandHandler _createHandler;
    private readonly UpdatePlanCommandHandler _updateHandler;
    private readonly GetPlanCatalogueQueryHandler _catalogueHandler;
    private readonly GetPlanDetailsQueryHandler _detailsHandler;
    private readonly IValidator<CreatePlanCommand> _createValidator;
    private readonly IValidator<UpdatePlanCommand> _updateValidator;

    public PlansController(
        CreatePlanCommandHandler createHandler,
        UpdatePlanCommandHandler updateHandler,
        GetPlanCatalogueQueryHandler catalogueHandler,
        GetPlanDetailsQueryHandler detailsHandler,
        IValidator<CreatePlanCommand> createValidator,
        IValidator<UpdatePlanCommand> updateValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _catalogueHandler = catalogueHandler;
        _detailsHandler = detailsHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalogue(
        [FromQuery] string? search,
        [FromQuery] int? branchId,
        [FromQuery] bool? isPublished,
        [FromQuery] int[]? durations,
        [FromQuery] AccessScope? accessScope,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetPlanCatalogueQuery(
                search,
                branchId,
                isPublished,
                durations,
                accessScope,
                minPrice,
                maxPrice,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize);

        var result =
            await _catalogueHandler.Handle(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("filter-options")]
    public async Task<IActionResult> GetFilterOptions(
        CancellationToken cancellationToken)
    {
        var result =
            await _catalogueHandler.GetFilterOptions(
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{planId:int}")]
    public async Task<IActionResult> GetById(
        int planId,
        CancellationToken cancellationToken)
    {
        var result =
            await _detailsHandler.Handle(
                new GetPlanDetailsQuery(
                    planId),
                cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new CreatePlanCommand(
                request.PlanName,
                request.Price,
                request.DurationInMonths,
                request.MaximumFreezeDays,
                request.MaximumNumberOfFreezes,
                request.GuestPassQuota,
                request.AccessScope,
                request.IsPublished,
                request.BranchIds);

        await _createValidator
            .ValidateAndThrowAsync(
                command,
                cancellationToken);

        var result =
            await _createHandler.Handle(
                command,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                planId = result.PlanId
            },
            result);
    }

    [HttpPut("{planId:int}")]
    public async Task<IActionResult> Update(
        int planId,
        [FromBody] UpdatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new UpdatePlanCommand(
                planId,
                request.PlanName,
                request.Price,
                request.DurationInMonths,
                request.MaximumFreezeDays,
                request.MaximumNumberOfFreezes,
                request.GuestPassQuota,
                request.AccessScope,
                request.IsPublished,
                request.BranchIds);

        await _updateValidator
            .ValidateAndThrowAsync(
                command,
                cancellationToken);

        await _updateHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }
}