using System.Collections.Generic;
using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Countries.Queries.GetCountries;

public record GetCountriesQuery : IRequest<List<CountryDto>>;
