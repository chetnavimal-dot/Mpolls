using System;

namespace MPolls.Domain.Entities;

public class RecommendedSurvey
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

    public string PanelistId { get; set; } = string.Empty;
}
