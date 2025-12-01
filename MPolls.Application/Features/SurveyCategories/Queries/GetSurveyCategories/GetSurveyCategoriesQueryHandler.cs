using System.Linq;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.SurveyCategories.Queries.GetSurveyCategories;

public class GetSurveyCategoriesQueryHandler
    : IRequestHandler<GetSurveyCategoriesQuery, List<SurveyCategoryDto>>
{
    private readonly ISurveyCategoryRepository _surveyCategoryRepository;

    public GetSurveyCategoriesQueryHandler(ISurveyCategoryRepository surveyCategoryRepository)
    {
        _surveyCategoryRepository = surveyCategoryRepository;
    }

    public async Task<List<SurveyCategoryDto>> Handle(
        GetSurveyCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _surveyCategoryRepository.GetActiveAsync(cancellationToken);

        return categories
            .Select(category => new SurveyCategoryDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Icon = category.Icon,
                IsActive = category.IsActive,
                RewardPoints = category.RewardPoints,
                RetakePoints = category.RetakePoints,
                RetakePointsIssueFrequency = category.RetakePointsIssueFrequency
            })
            .ToList();
    }
}
