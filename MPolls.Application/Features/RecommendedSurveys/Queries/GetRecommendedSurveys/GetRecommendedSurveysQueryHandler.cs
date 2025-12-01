using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Queries.GetRecommendedSurveys;

public sealed class GetRecommendedSurveysQueryHandler : IRequestHandler<GetRecommendedSurveysQuery, IReadOnlyList<RecommendedSurveyDto>>
{
    private readonly IRecommendedSurveyRepository _recommendedSurveyRepository;

    public GetRecommendedSurveysQueryHandler(IRecommendedSurveyRepository recommendedSurveyRepository)
    {
        _recommendedSurveyRepository = recommendedSurveyRepository;
    }

    public async Task<IReadOnlyList<RecommendedSurveyDto>> Handle(GetRecommendedSurveysQuery request, CancellationToken cancellationToken)
    {
        var surveys = await _recommendedSurveyRepository.GetByPanelistIdAsync(request.PanelistId, request.IncludeCompleted, cancellationToken);
        return surveys.Select(RecommendedSurveyDto.FromEntity).ToList();
    }
}
