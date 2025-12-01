using System;
using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.API.Models.Survey;
using MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;
using MPolls.Application.Features.ProfileQuestions.Commands.SaveSurveyResults;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class SurveyResultsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SurveyResultsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<EngageRewardResponse>> Save(
        [FromBody] SaveSurveyResultsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var authenticatedFirebaseId = GetAuthenticatedFirebaseId();

        if (string.IsNullOrWhiteSpace(authenticatedFirebaseId))
        {
            return Unauthorized();
        }

        var panelist = await _mediator.Send(new GetPanelistSummaryQuery(authenticatedFirebaseId), cancellationToken);

        if (panelist is null || string.IsNullOrWhiteSpace(panelist.Ulid))
        {
            return Forbid();
        }

        var command = new SaveSurveyResultsCommand(panelist.Ulid, request.CategoryId, request.SurveyJson);
        var result = await _mediator.Send(command, cancellationToken);

        var response = new EngageRewardResponse
        {
            PointsCollected = result.PointsCollected
        };

        return Ok(response);
    }

    private string? GetAuthenticatedFirebaseId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
    }
}
