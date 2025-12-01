using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Countries.Queries.GetCountryByCode;

public class GetCountryByCodeQueryHandler : IRequestHandler<GetCountryByCodeQuery, CountryDto?>
{
    private readonly ICountryRepository _countryRepository;

    public GetCountryByCodeQueryHandler(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<CountryDto?> Handle(GetCountryByCodeQuery request, CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByCodeAsync(request.CountryCode, cancellationToken);

        if (country is null)
        {
            return null;
        }

        return new CountryDto
        {
            CountryCode = country.CountryCode,
            CountryShortCode = country.CountryShortCode,
            CountryName = country.CountryName
        };
    }
}
