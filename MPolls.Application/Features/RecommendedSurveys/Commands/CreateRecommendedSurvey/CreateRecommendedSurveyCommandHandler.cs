using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Surveys;
using MPolls.Domain.Entities;

namespace MPolls.Application.Features.RecommendedSurveys.Commands.CreateRecommendedSurvey;

public sealed class CreateRecommendedSurveyCommandHandler : IRequestHandler<CreateRecommendedSurveyCommand, RecommendedSurveyDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRecommendedSurveyRepository _recommendedSurveyRepository;

    public CreateRecommendedSurveyCommandHandler(
        IApplicationDbContext dbContext,
        IRecommendedSurveyRepository recommendedSurveyRepository)
    {
        _dbContext = dbContext;
        _recommendedSurveyRepository = recommendedSurveyRepository;
    }

    public async Task<RecommendedSurveyDto?> Handle(CreateRecommendedSurveyCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SurveyName) || string.IsNullOrWhiteSpace(request.SurveyLink))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.PanelistId))
        {
            return null;
        }

        var panelistUlid = request.PanelistId.Trim();

        var panelistExists = await _dbContext.Panelists
            .AsNoTracking()
            .AnyAsync(panelist => panelist.Ulid == panelistUlid, cancellationToken);

        if (!panelistExists)
        {
            return null;
        }

        var survey = new RecommendedSurvey
        {
            Id = Guid.NewGuid(),
            PanelistId = panelistUlid,
            SurveyName = request.SurveyName.Trim(),
            SurveyDescription = string.IsNullOrWhiteSpace(request.SurveyDescription)
                ? null
                : request.SurveyDescription.Trim(),
            SurveyLink = request.SurveyLink.Trim(),
            ExpiringOn = NormalizeDate(request.ExpiringOn),
            EstimatedRewardPoints = Math.Max(0, request.EstimatedRewardPoints),
            MultipleResponseAllowed = request.MultipleResponseAllowed,
            AssignedOn = NormalizeAssignedOn(request.AssignedOn)
        };

        await _recommendedSurveyRepository.AddAsync(survey, cancellationToken);

        return RecommendedSurveyDto.FromEntity(survey);
    }

    private static DateTime NormalizeAssignedOn(DateTime? assignedOn)
    {
        var value = assignedOn ?? DateTime.UtcNow;
        return NormalizeToUtc(value);
    }

    private static DateTime? NormalizeDate(DateTime? date)
    {
        if (!date.HasValue)
        {
            return null;
        }

        return NormalizeToUtc(date.Value);
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
