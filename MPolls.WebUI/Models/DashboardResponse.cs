using MPolls.WebUI.Models.Survey;

namespace MPolls.WebUI.Models;

public class DashboardResponse
{
    public List<RecommendedSurveyModel> ActiveSurveys { get; set; }
    public int RewardBalance { get; set; }
    public int ProfileStrength { get; set; }
    public int MonthlySurveys { get; set; }
}