using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.Application.Features.SurveyCategories.Queries.GetSurveyCategories;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SurveyCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SurveyCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _mediator.Send(new GetSurveyCategoriesQuery());
        return Ok(categories);
    }
}
