using Microsoft.AspNetCore.Mvc;

using TitanFitness.Application.Branches.Queries.GetBranches;
using TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly GetBranchesQueryHandler _getBranchesHandler;
    private readonly GetStudiosByBranchQueryHandler _getStudiosHandler;

    public BranchesController(
        GetBranchesQueryHandler getBranchesHandler,
        GetStudiosByBranchQueryHandler getStudiosHandler)
    {
        _getBranchesHandler = getBranchesHandler;
        _getStudiosHandler = getStudiosHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches(
        CancellationToken cancellationToken)
    {
        var result = await _getBranchesHandler.Handle(
            new GetBranchesQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{branchId:int}/studios")]
    public async Task<IActionResult> GetStudios(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _getStudiosHandler.Handle(
            new GetStudiosByBranchQuery(branchId),
            cancellationToken);

        return Ok(result);
    }
}