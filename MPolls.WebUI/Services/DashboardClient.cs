using System.Net.Http.Json;
using MPolls.WebUI.Models;

namespace MPolls.WebUI.Services;

public class DashboardClient
{
    private readonly HttpClient _httpClient;

    public DashboardClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DashboardResponse?> GetDashboardDetails(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DashboardResponse>("api/v1/Dashboard/summary", cancellationToken);
    }
}