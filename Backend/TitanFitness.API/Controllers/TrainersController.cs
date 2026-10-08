using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.API.Contracts.Trainers;
using TitanFitness.Application.Trainers.Commands.CreateTrainer;
using TitanFitness.Application.Trainers.Commands.UpdateTrainer;
using TitanFitness.Application.Trainers.Queries.GetTrainerDetails;
using TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;
using TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/trainers")]
public sealed class TrainersController : ControllerBase
{
    private readonly CreateTrainerCommandHandler _createHandler;
    private readonly UpdateTrainerCommandHandler _updateHandler;
    private readonly GetTrainerDirectoryQueryHandler _directoryHandler;
    private readonly GetTrainerDetailsQueryHandler _detailsHandler;
    private readonly GetTrainerLookupQueryHandler _lookupHandler;
    private readonly IValidator<CreateTrainerCommand> _createValidator;
    private readonly IValidator<UpdateTrainerCommand> _updateValidator;

    public TrainersController(
        CreateTrainerCommandHandler createHandler,
        UpdateTrainerCommandHandler updateHandler,
        GetTrainerDirectoryQueryHandler directoryHandler,
        GetTrainerDetailsQueryHandler detailsHandler,
        GetTrainerLookupQueryHandler lookupHandler,
        IValidator<CreateTrainerCommand> createValidator,
        IValidator<UpdateTrainerCommand> updateValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _directoryHandler = directoryHandler;
        _detailsHandler = detailsHandler;
        _lookupHandler = lookupHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDirectory(
        [FromQuery] string? search,
        [FromQuery] int[]? branchIds,
        [FromQuery] string[]? specialties,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrainerDirectoryQuery(
            search,
            branchIds,
            specialties,
            isActive,
            sortBy,
            sortDirection,
            pageNumber,
            pageSize);

        var result = await _directoryHandler.Handle(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties(
        CancellationToken cancellationToken)
    {
        var result = await _directoryHandler.GetSpecialties(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{trainerId:int}")]
    public async Task<IActionResult> GetById(
        int trainerId,
        CancellationToken cancellationToken)
    {
        var result = await _detailsHandler.Handle(
            new GetTrainerDetailsQuery(trainerId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup(
        [FromQuery] int branchId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _lookupHandler.Handle(
            new GetTrainerLookupQuery(
                branchId,
                search),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTrainerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTrainerCommand(
            request.TrainerName,
            request.Specialty,
            request.Email,
            request.Phone,
            request.BranchId,
            request.IsActive);

        await _createValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var result = await _createHandler.Handle(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { trainerId = result.TrainerId },
            result);
    }

    [HttpPut("{trainerId:int}")]
    public async Task<IActionResult> Update(
        int trainerId,
        [FromBody] UpdateTrainerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTrainerCommand(
            trainerId,
            request.TrainerName,
            request.Specialty,
            request.Email,
            request.Phone,
            request.BranchId,
            request.IsActive);

        await _updateValidator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        await _updateHandler.Handle(
            command,
            cancellationToken);

        return NoContent();
    }
}