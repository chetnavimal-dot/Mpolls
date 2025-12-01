using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Countries.Queries.GetCountryByCode;

public record GetCountryByCodeQuery(int CountryCode) : IRequest<CountryDto?>;
