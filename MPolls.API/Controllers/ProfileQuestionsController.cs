using Asp.Versioning;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;
using MPolls.Application.Features.ProfileQuestions.Queries.GetPanelistProfileDetails;
using MPolls.Application.Features.ProfileQuestions.Queries.GetProfileQuestions;
using MPolls.Domain.Enums;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProfileQuestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileQuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{category}")]
    public async Task<IActionResult> Get(ProfileQuestionCategory category, CancellationToken cancellationToken)
    {
        var survey = await _mediator.Send(new GetProfileQuestionsQuery(category), cancellationToken);
        return Ok(survey);
    }

    [HttpGet("{category}/responses")]
    public async Task<IActionResult> GetResponses(ProfileQuestionCategory category, CancellationToken cancellationToken)
    {
        var firebaseId = GetAuthenticatedFirebaseId();

        if (string.IsNullOrWhiteSpace(firebaseId))
        {
            return Unauthorized();
        }

        var panelist = await _mediator.Send(new GetPanelistSummaryQuery(firebaseId), cancellationToken);

        if (panelist is null)
        {
            return Forbid();
        }

        var query = new GetPanelistProfileDetailsQuery(panelist.Ulid, (int)category);
        var details = await _mediator.Send(query, cancellationToken);

        return Ok(details);
    }

    private string? GetAuthenticatedFirebaseId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
    }
}
