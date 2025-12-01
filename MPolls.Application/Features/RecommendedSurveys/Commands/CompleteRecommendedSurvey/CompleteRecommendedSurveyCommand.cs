using System;
using MediatR;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Commands.CompleteRecommendedSurvey;

public sealed record CompleteRecommendedSurveyCommand(Guid Id, DateTime? CompletedOn) : IRequest<RecommendedSurveyDto?>;
