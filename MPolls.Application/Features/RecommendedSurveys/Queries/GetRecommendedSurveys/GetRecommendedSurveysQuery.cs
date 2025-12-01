using System.Collections.Generic;
using MediatR;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Queries.GetRecommendedSurveys;

public sealed record GetRecommendedSurveysQuery(string PanelistId, bool IncludeCompleted) : IRequest<IReadOnlyList<RecommendedSurveyDto>>;
