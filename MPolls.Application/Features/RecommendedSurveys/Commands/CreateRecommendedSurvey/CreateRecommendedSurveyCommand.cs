using System;
using MediatR;
using MPolls.Application.DTOs.Surveys;

namespace MPolls.Application.Features.RecommendedSurveys.Commands.CreateRecommendedSurvey;

public sealed record CreateRecommendedSurveyCommand(
    string PanelistId,
    string SurveyName,
    string? SurveyDescription,
    string SurveyLink,
    DateTime? ExpiringOn,
    int EstimatedRewardPoints,
    bool MultipleResponseAllowed,
    DateTime? AssignedOn) : IRequest<RecommendedSurveyDto?>;
