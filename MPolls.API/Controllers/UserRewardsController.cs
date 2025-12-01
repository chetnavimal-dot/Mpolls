using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;
using MPolls.Application.Features.UserRewards.Queries;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UserRewardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserRewardsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserRewards(CancellationToken cancellationToken)
    {
        var firebaseId = GetAuthenticatedFirebaseId();

        if (string.IsNullOrWhiteSpace(firebaseId))
        {
            return Unauthorized();
        }

        var panelist = await _mediator.Send(new GetPanelistSummaryQuery(firebaseId), cancellationToken);

        if (panelist is null)
        {
            return NotFound();
        }

        var response = await _mediator.Send(new GetUserRewardsQuery(panelist.Ulid), cancellationToken);

        return Ok(response);
    }

    private string? GetAuthenticatedFirebaseId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
    }
}