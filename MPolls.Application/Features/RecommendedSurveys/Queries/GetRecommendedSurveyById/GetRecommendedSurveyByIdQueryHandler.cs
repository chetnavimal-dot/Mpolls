using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Queries.GetRecommendedSurveyById;

public sealed class GetRecommendedSurveyByIdQueryHandler : IRequestHandler<GetRecommendedSurveyByIdQuery, RecommendedSurveyDto?>
{
    private readonly IRecommendedSurveyRepository _recommendedSurveyRepository;

    public GetRecommendedSurveyByIdQueryHandler(IRecommendedSurveyRepository recommendedSurveyRepository)
    {
        _recommendedSurveyRepository = recommendedSurveyRepository;
    }

    public async Task<RecommendedSurveyDto?> Handle(GetRecommendedSurveyByIdQuery request, CancellationToken cancellationToken)
    {
        var survey = await _recommendedSurveyRepository.GetByIdAsync(request.Id, cancellationToken);
        return survey is null ? null : RecommendedSurveyDto.FromEntity(survey);
    }
}
