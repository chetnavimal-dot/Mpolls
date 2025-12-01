using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MPolls.WebUI.Models;

namespace MPolls.WebUI.Services;

public class SurveyCategoriesClient
{
    private readonly HttpClient _httpClient;

    public SurveyCategoriesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SurveyCategoryModel>> GetSurveyCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _httpClient.GetFromJsonAsync<List<SurveyCategoryModel>>("api/v1/SurveyCategories", cancellationToken);

            return categories?.OrderBy(category => category.CategoryName).ToList() ?? new List<SurveyCategoryModel>();
        }
        catch
        {
            return new List<SurveyCategoryModel>();
        }
    }
}
