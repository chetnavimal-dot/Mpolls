using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Profile;
using MPolls.Domain.Entities;

namespace MPolls.Application.Features.ProfileQuestions.Queries.GetPanelistProfileDetails;

public sealed class GetPanelistProfileDetailsQueryHandler : IRequestHandler<GetPanelistProfileDetailsQuery, ProfileSurveyDetailsDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISurveyQuestionRepository _surveyQuestionRepository;

    public GetPanelistProfileDetailsQueryHandler(
        IApplicationDbContext dbContext,
        ISurveyQuestionRepository surveyQuestionRepository)
    {
        _dbContext = dbContext;
        _surveyQuestionRepository = surveyQuestionRepository;
    }

    public async Task<ProfileSurveyDetailsDto> Handle(GetPanelistProfileDetailsQuery request, CancellationToken cancellationToken)
    {
        var questions = await _surveyQuestionRepository.GetByCategoryAsync(request.CategoryId, cancellationToken);

        var details = new ProfileSurveyDetailsDto
        {
            CategoryId = request.CategoryId,
            TotalQuestionCount = questions.Count,
            LastResponseOn = null,
            Responses = Array.Empty<ProfileQuestionDetailDto>()
        };

        if (questions.Count == 0)
        {
            return details;
        }

        if (string.IsNullOrWhiteSpace(request.PanelistId))
        {
            return details;
        }

        var responses = await _dbContext.PanelistProfiles
            .AsNoTracking()
            .Where(profile => profile.PanelistId == request.PanelistId && profile.CategoryId == request.CategoryId)
            .ToListAsync(cancellationToken);

        if (responses.Count == 0)
        {
            return details;
        }

        var lastResponseOn = responses.Max(profile => profile.CreatedOn);

        var questionLookup = questions.ToDictionary(question => question.QuestionId);

        var questionDetails = new List<ProfileQuestionDetailDto>();

        foreach (var grouping in responses
            .Where(profile => questionLookup.ContainsKey(profile.QuestionId))
            .GroupBy(profile => profile.QuestionId))
        {
            if (!questionLookup.TryGetValue(grouping.Key, out var question))
            {
                continue;
            }

            var answers = new List<ProfileAnswerDetailDto>();

            foreach (var entry in grouping.OrderBy(profile => profile.MatrixQuestionId ?? 0))
            {
                var representation = CreateAnswerRepresentation(entry, question);

                if (string.IsNullOrWhiteSpace(representation))
                {
                    continue;
                }

                string? matrixRow = null;

                if (entry.MatrixQuestionId.HasValue)
                {
                    matrixRow = question.MatrixOptions
                        .FirstOrDefault(option => option.MatrixRowId == entry.MatrixQuestionId.Value)?.MatrixRowText;
                }

                answers.Add(new ProfileAnswerDetailDto
                {
                    MatrixRow = matrixRow,
                    Value = representation
                });
            }

            if (answers.Count == 0)
            {
                continue;
            }

            questionDetails.Add(new ProfileQuestionDetailDto
            {
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                Answers = answers
            });
        }

        details = new ProfileSurveyDetailsDto
        {
            CategoryId = request.CategoryId,
            TotalQuestionCount = questions.Count,
            LastResponseOn = lastResponseOn,
            Responses = questionDetails
        };

        return details;
    }

    private static string? CreateAnswerRepresentation(PanelistProfile entry, SurveyQuestion question)
    {
        if (!string.IsNullOrWhiteSpace(entry.Text))
        {
            return entry.Text;
        }

        if (entry.Numeric.HasValue)
        {
            return entry.Numeric.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (entry.DateTime.HasValue)
        {
            return entry.DateTime.Value.ToString("u", CultureInfo.InvariantCulture);
        }

        if (string.IsNullOrWhiteSpace(entry.AnswerIds))
        {
            return null;
        }

        var answers = new List<string>();

        foreach (var rawAnswer in entry.AnswerIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (long.TryParse(rawAnswer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var optionId))
            {
                var option = question.Options.FirstOrDefault(opt => opt.OptionId == optionId);

                if (option is not null && !string.IsNullOrWhiteSpace(option.OptionText))
                {
                    answers.Add(option.OptionText);
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(rawAnswer))
            {
                answers.Add(rawAnswer);
            }
        }

        if (answers.Count == 0)
        {
            return null;
        }

        return string.Join(", ", answers);
    }
}
