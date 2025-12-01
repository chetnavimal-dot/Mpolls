using System;

namespace MPolls.WebUI.Models.Survey;

public class RecommendedSurveyModel
{
    public Guid Id { get; set; }

    public string SurveyName { get; set; } = string.Empty;

    public string? SurveyDescription { get; set; }

    public string SurveyLink { get; set; } = string.Empty;

    public DateTime? ExpiringOn { get; set; }

    public int EstimatedRewardPoints { get; set; }

    public bool MultipleResponseAllowed { get; set; }

    public DateTime AssignedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public bool IsCompleted => CompletedOn.HasValue;
}
