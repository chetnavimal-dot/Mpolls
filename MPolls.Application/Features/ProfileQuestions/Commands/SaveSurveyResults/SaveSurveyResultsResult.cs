namespace MPolls.Application.Features.ProfileQuestions.Commands.SaveSurveyResults;

public sealed record SaveSurveyResultsResult(int PointsCollected)
{
    public static SaveSurveyResultsResult Empty { get; } = new(0);
}
