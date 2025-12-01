using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.DTOs;

public class DashboardResponse
{
    public List<RecommendedSurveyDto> ActiveSurveys { get; set; }
    public int RewardBalance { get; set; }
    public int ProfileStrength { get; set; }
    public int MonthlySurveys { get; set; }
}