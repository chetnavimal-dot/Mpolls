using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.API.Models.Panelists;
using MPolls.Application.Features.Panelists.Commands.CompleteOnboarding;
using MPolls.Application.Features.Panelists.Queries.GetPanelistUlid;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Panelist")]
[Authorize]
public sealed class PanelistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PanelistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<ActionResult<PanelistUlidResponse>> GetUlid(CancellationToken cancellationToken)
    {
        var authenticatedFirebaseId = GetAuthenticatedFirebaseId();

        if (string.IsNullOrWhiteSpace(authenticatedFirebaseId))
        {
            return Unauthorized();
        }

        var ulid = await _mediator.Send(new GetPanelistUlidQuery(authenticatedFirebaseId), cancellationToken);

        if (string.IsNullOrWhiteSpace(ulid))
        {
            return NotFound();
        }

        return Ok(new PanelistUlidResponse(ulid));
    }

    [HttpPut("completeOnboarding")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var authenticatedFirebaseId = GetAuthenticatedFirebaseId();

        if (string.IsNullOrWhiteSpace(authenticatedFirebaseId))
        {
            return Unauthorized();
        }

        var completed = await _mediator.Send(new CompletePanelistOnboardingCommand(authenticatedFirebaseId,request.Age,request.Gender,request.CountryCode), cancellationToken);

        if (!completed)
        {
            return NotFound();
        }

        return NoContent();
    }

    private string? GetAuthenticatedFirebaseId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
    }
}
