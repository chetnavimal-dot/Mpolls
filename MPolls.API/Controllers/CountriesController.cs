using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.Application.Features.Countries.Queries.GetCountries;
using MPolls.Application.Features.Countries.Queries.GetCountryByCode;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var countries = await _mediator.Send(new GetCountriesQuery());
        return Ok(countries);
    }

    [HttpGet("{countryCode:int}")]
    public async Task<IActionResult> GetByCode(int countryCode)
    {
        var country = await _mediator.Send(new GetCountryByCodeQuery(countryCode));

        if (country is null)
        {
            return NotFound();
        }

        return Ok(country);
    }
}
