using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models.Survey;

namespace MPolls.WebUI.Services;

public class SurveyResultsClient
{
    private readonly HttpClient _httpClient;

    public SurveyResultsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EngageRewardResponseModel?> SaveSurveyResultsAsync(
        SaveSurveyResultsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/v1/SurveyResults", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EngageRewardResponseModel>(cancellationToken: cancellationToken);
    }
}
