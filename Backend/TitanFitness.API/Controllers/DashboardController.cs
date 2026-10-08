using Microsoft.AspNetCore.Mvc;

using TitanFitness.Application.Dashboard.Queries.GetDashboard;

namespace TitanFitness.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly GetDashboardQueryHandler _handler;

    public DashboardController(
        GetDashboardQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? branchId,
        CancellationToken cancellationToken)
    {
        var result = await _handler.Handle(
            new GetDashboardQuery(branchId),
            cancellationToken);

        return Ok(result);
    }
}