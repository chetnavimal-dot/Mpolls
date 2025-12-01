using System;
using MediatR;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Queries.GetRecommendedSurveyById;

public sealed record GetRecommendedSurveyByIdQuery(Guid Id) : IRequest<RecommendedSurveyDto?>;
