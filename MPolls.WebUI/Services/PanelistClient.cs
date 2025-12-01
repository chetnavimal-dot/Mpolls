using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models.Panelists;

namespace MPolls.WebUI.Services;

public class PanelistClient
{
    private readonly HttpClient _httpClient;

    public PanelistClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetPanelistUlidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PanelistUlidResponse>(
                "api/v1/Panelist/me",
                cancellationToken);

            var ulid = response?.Ulid?.Trim();
            return string.IsNullOrWhiteSpace(ulid) ? null : ulid;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CompleteOnboardingAsync(CompleteOnboardingRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            "api/v1/Panelist/completeOnboarding",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
