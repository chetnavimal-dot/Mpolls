using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Countries.Queries.GetCountries;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, List<CountryDto>>
{
    private readonly ICountryRepository _countryRepository;

    public GetCountriesQueryHandler(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<List<CountryDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetActiveAsync(cancellationToken);

        return countries
            .Select(country => new CountryDto
            {
                CountryCode = country.CountryCode,
                CountryShortCode = country.CountryShortCode,
                CountryName = country.CountryName
            })
            .ToList();
    }
}
