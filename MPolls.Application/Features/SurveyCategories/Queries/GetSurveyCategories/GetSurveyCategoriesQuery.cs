using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.SurveyCategories.Queries.GetSurveyCategories;

public record GetSurveyCategoriesQuery : IRequest<List<SurveyCategoryDto>>;
