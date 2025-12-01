namespace MPolls.WebUI.Models.Survey;

public sealed class SaveSurveyResultsRequest
{
    public int CategoryId { get; set; }

    public string SurveyJson { get; set; } = string.Empty;
}
