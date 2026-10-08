using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.API.Contracts.Members;
using TitanFitness.Application.Members.Commands.CreateMember;
using TitanFitness.Application.Members.Commands.UpdateMember;
using TitanFitness.Application.Members.Queries.GetMemberDirectory;
using TitanFitness.Application.Members.Queries.GetMemberProfile;
using TitanFitness.Application.Members.Queries.SearchMembersLookup;
using TitanFitness.Domain.Enums;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersController : ControllerBase
{
    private readonly CreateMemberCommandHandler _createHandler;
    private readonly UpdateMemberCommandHandler _updateHandler;
    private readonly GetMemberDirectoryQueryHandler _directoryHandler;
    private readonly GetMemberProfileQueryHandler _profileHandler;
    private readonly SearchMembersLookupQueryHandler _lookupHandler;
    private readonly IValidator<CreateMemberCommand> _createValidator;
    private readonly IValidator<UpdateMemberCommand> _updateValidator;

    public MembersController(
        CreateMemberCommandHandler createHandler,
        UpdateMemberCommandHandler updateHandler,
        GetMemberDirectoryQueryHandler directoryHandler,
        GetMemberProfileQueryHandler profileHandler,
        SearchMembersLookupQueryHandler lookupHandler,
        IValidator<CreateMemberCommand> createValidator,
        IValidator<UpdateMemberCommand> updateValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _directoryHandler = directoryHandler;
        _profileHandler = profileHandler;
        _lookupHandler = lookupHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDirectory(
        [FromQuery] string? search,
        [FromQuery] int[]? branchIds,
        [FromQuery] MembershipStatus[]? statuses,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMemberDirectoryQuery(
            search,
            branchIds,
            statuses,
            sortBy,
            sortDirection,
            pageNumber,
            pageSize);

        var result = await _directoryHandler.Handle(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string? search,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchMembersLookupQuery(
            search,
            limit);

        var result = await _lookupHandler.Handle(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{memberId:int}")]
    public async Task<IActionResult> GetProfile(
        int memberId,
        CancellationToken cancellationToken)
    {
        var result = await _profileHandler.Handle(
            new GetMemberProfileQuery(memberId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        Stream? photoStream = null;

        try
        {
            if (request.Photo is not null)
            {
                photoStream = request.Photo.OpenReadStream();
            }

            var joinedDate = request.JoinedDate == default
                ? DateOnly.FromDateTime(DateTime.Now)
                : request.JoinedDate;

            var command = new CreateMemberCommand(
                request.MembershipNumber,
                request.FullName,
                request.Email,
                request.Phone,
                request.Address,
                joinedDate,
                request.HomeBranchId,
                photoStream,
                request.Photo?.FileName);

            await _createValidator.ValidateAndThrowAsync(
                command,
                cancellationToken);

            var result = await _createHandler.Handle(
                command,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetProfile),
                new { memberId = result.MemberId },
                result);
        }
        finally
        {
            if (photoStream is not null)
            {
                await photoStream.DisposeAsync();
            }
        }
    }

    [HttpPut("{memberId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int memberId,
        [FromForm] UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        Stream? photoStream = null;

        try
        {
            if (request.Photo is not null)
            {
                photoStream = request.Photo.OpenReadStream();
            }

            var command = new UpdateMemberCommand(
                memberId,
                request.FullName,
                request.Email,
                request.Phone,
                request.Address,
                request.JoinedDate,
                request.HomeBranchId,
                photoStream,
                request.Photo?.FileName);

            await _updateValidator.ValidateAndThrowAsync(
                command,
                cancellationToken);

            await _updateHandler.Handle(
                command,
                cancellationToken);

            return NoContent();
        }
        finally
        {
            if (photoStream is not null)
            {
                await photoStream.DisposeAsync();
            }
        }
    }
}