using System;
using MPolls.Domain.Entities;

namespace MPolls.Application.DTOs.Surveys;

public class RecommendedSurveyDto
{
    public Guid Id { get; init; }

    public string SurveyName { get; init; } = string.Empty;

    public string? SurveyDescription { get; init; }

    public string SurveyLink { get; init; } = string.Empty;

    public DateTime? ExpiringOn { get; init; }

    public int EstimatedRewardPoints { get; init; }

    public bool MultipleResponseAllowed { get; init; }

    public DateTime AssignedOn { get; init; }

    public DateTime? CompletedOn { get; init; }

    public static RecommendedSurveyDto FromEntity(RecommendedSurvey survey)
    {
        return new RecommendedSurveyDto
        {
            Id = survey.Id,
            SurveyName = survey.SurveyName,
            SurveyDescription = survey.SurveyDescription,
            SurveyLink = survey.SurveyLink,
            ExpiringOn = survey.ExpiringOn,
            EstimatedRewardPoints = survey.EstimatedRewardPoints,
            MultipleResponseAllowed = survey.MultipleResponseAllowed,
            AssignedOn = survey.AssignedOn,
            CompletedOn = survey.CompletedOn
        };
    }
}
