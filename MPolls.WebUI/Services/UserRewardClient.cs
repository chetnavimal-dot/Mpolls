using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models;

namespace MPolls.WebUI.Services;

public class UserRewardClient
{
    private readonly HttpClient _httpClient;

    public UserRewardClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<RewardResponse?> GetUserRewardsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RewardResponse>("api/v1/UserRewards",cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }
}