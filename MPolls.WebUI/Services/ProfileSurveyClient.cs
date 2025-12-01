using System.Net.Http.Json;
using MPolls.WebUI.Models.Survey;

namespace MPolls.WebUI.Services;

public class ProfileSurveyClient
{
    private readonly HttpClient _httpClient;

    public ProfileSurveyClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SurveyJsSurveyModel?> GetProfileSurveyAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SurveyJsSurveyModel>($"api/v1/ProfileQuestions/{categoryId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ProfileSurveyDetailsModel?> GetProfileSurveyDetailsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProfileSurveyDetailsModel>($"api/v1/ProfileQuestions/{categoryId}/responses", cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
