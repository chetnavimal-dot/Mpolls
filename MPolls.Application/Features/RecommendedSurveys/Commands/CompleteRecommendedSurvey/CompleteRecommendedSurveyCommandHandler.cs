using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Commands.CompleteRecommendedSurvey;

public sealed class CompleteRecommendedSurveyCommandHandler : IRequestHandler<CompleteRecommendedSurveyCommand, RecommendedSurveyDto?>
{
    private readonly IRecommendedSurveyRepository _recommendedSurveyRepository;

    public CompleteRecommendedSurveyCommandHandler(IRecommendedSurveyRepository recommendedSurveyRepository)
    {
        _recommendedSurveyRepository = recommendedSurveyRepository;
    }

    public async Task<RecommendedSurveyDto?> Handle(CompleteRecommendedSurveyCommand request, CancellationToken cancellationToken)
    {
        var survey = await _recommendedSurveyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (survey is null)
        {
            return null;
        }

        if (request.CompletedOn.HasValue)
        {
            survey.CompletedOn = NormalizeCompletion(request.CompletedOn.Value);
        }
        else if (!survey.CompletedOn.HasValue)
        {
            survey.CompletedOn = NormalizeCompletion(DateTime.UtcNow);
        }

        await _recommendedSurveyRepository.UpdateAsync(survey, cancellationToken);

        return RecommendedSurveyDto.FromEntity(survey);
    }

    private static DateTime NormalizeCompletion(DateTime completionDate)
    {
        return completionDate.Kind switch
        {
            DateTimeKind.Utc => completionDate,
            DateTimeKind.Local => completionDate.ToUniversalTime(),
            _ => DateTime.SpecifyKind(completionDate, DateTimeKind.Utc)
        };
    }
}
