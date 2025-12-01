using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.API.Models.Survey;
using MPolls.Application.DTOs.Surveys;
using MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;
using MPolls.Application.Features.RecommendedSurveys.Commands.CompleteRecommendedSurvey;
using MPolls.Application.Features.RecommendedSurveys.Commands.CreateRecommendedSurvey;
using MPolls.Application.Features.RecommendedSurveys.Queries.GetRecommendedSurveys;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class RecommendedSurveysController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecommendedSurveysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeCompleted = false, CancellationToken cancellationToken = default)
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

        var surveys = await _mediator.Send(new GetRecommendedSurveysQuery(panelist.Ulid, includeCompleted), cancellationToken);
        return Ok(surveys);
    }

    [HttpPost]
    public async Task<ActionResult<RecommendedSurveyDto>> Create([FromBody] CreateRecommendedSurveyCommand command, CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(command.SurveyName) || string.IsNullOrWhiteSpace(command.SurveyLink))
        {
            return BadRequest();
        }

        var survey = await _mediator.Send(command, cancellationToken);

        if (survey is null)
        {
            return NotFound();
        }

        return Ok(survey);
    }

    [HttpPut("{id:guid}/completion")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteRecommendedSurveyRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var command = new CompleteRecommendedSurveyCommand(id, request.CompletedOn);
        var survey = await _mediator.Send(command, cancellationToken);

        return survey is null ? NotFound() : Ok(survey);
    }

    private string? GetAuthenticatedFirebaseId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
    }
}
