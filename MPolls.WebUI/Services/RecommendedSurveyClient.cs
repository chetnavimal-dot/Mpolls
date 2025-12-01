using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models.Survey;

namespace MPolls.WebUI.Services;

public class RecommendedSurveyClient
{
    private readonly HttpClient _httpClient;

    public RecommendedSurveyClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RecommendedSurveyModel>> GetRecommendedSurveysAsync(
        bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/v1/RecommendedSurveys?includeCompleted={includeCompleted.ToString().ToLowerInvariant()}";
        var surveys = await _httpClient.GetFromJsonAsync<List<RecommendedSurveyModel>>(endpoint, cancellationToken);

        return surveys ?? new List<RecommendedSurveyModel>();
    }

    public async Task<RecommendedSurveyModel?> MarkSurveyCompletedAsync(
        Guid surveyId,
        DateTime? completedOn = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RecommendedSurveyCompletionRequest
        {
            CompletedOn = completedOn ?? DateTime.UtcNow
        };

        using var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/RecommendedSurveys/{surveyId}/completion",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RecommendedSurveyModel>(cancellationToken: cancellationToken);
    }

    private sealed class RecommendedSurveyCompletionRequest
    {
        public DateTime? CompletedOn { get; set; }
    }
}
