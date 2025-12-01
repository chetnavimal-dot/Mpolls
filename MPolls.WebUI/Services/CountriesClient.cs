using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models;

namespace MPolls.WebUI.Services;

public class CountriesClient
{
    private readonly HttpClient _httpClient;

    public CountriesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CountryModel>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var countries = await _httpClient.GetFromJsonAsync<List<CountryModel>>("api/v1/Countries", cancellationToken);

            return countries?.OrderBy(country => country.CountryName).ToList() ?? new List<CountryModel>();
        }
        catch
        {
            return new List<CountryModel>();
        }
    }

    public async Task<CountryModel?> GetCountryAsync(int countryCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CountryModel>($"api/v1/Countries/{countryCode}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
