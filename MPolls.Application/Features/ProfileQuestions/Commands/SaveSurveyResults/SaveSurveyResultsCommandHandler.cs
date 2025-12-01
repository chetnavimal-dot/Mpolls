using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;
using MPolls.Domain.Enums;

namespace MPolls.Application.Features.ProfileQuestions.Commands.SaveSurveyResults;

public sealed class SaveSurveyResultsCommandHandler : IRequestHandler<SaveSurveyResultsCommand, SaveSurveyResultsResult>
{
    private readonly IApplicationDbContext _dbContext;

    public SaveSurveyResultsCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaveSurveyResultsResult> Handle(SaveSurveyResultsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Ulid))
        {
            return SaveSurveyResultsResult.Empty;
        }

        if (string.IsNullOrWhiteSpace(request.SurveyJson))
        {
            return SaveSurveyResultsResult.Empty;
        }

        var panelistExists = await _dbContext.Panelists
            .AsNoTracking()
            .AnyAsync(panelist => panelist.Ulid == request.Ulid, cancellationToken);

        if (!panelistExists)
        {
            return SaveSurveyResultsResult.Empty;
        }

        var category = await _dbContext.SurveyCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(cat => cat.CategoryId == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return SaveSurveyResultsResult.Empty;
        }

        var questions = await _dbContext.SurveyQuestions
            .AsNoTracking()
            .Where(question => question.CategoryId == request.CategoryId)
            .ToDictionaryAsync(question => question.QuestionId, cancellationToken);

        if (questions.Count == 0)
        {
            return SaveSurveyResultsResult.Empty;
        }

        var createdOn = DateTime.UtcNow;

        var lastSubmission = await _dbContext.PanelistProfiles
            .AsNoTracking()
            .Where(profile => profile.PanelistId == request.Ulid && profile.CategoryId == request.CategoryId)
            .Select(profile => (DateTime?)profile.CreatedOn)
            .OrderByDescending(timestamp => timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        List<PanelistProfile> entries;

        try
        {
            entries = ExtractEntries(request.SurveyJson, request.Ulid, request.CategoryId, questions, createdOn);
        }
        catch (JsonException)
        {
            return SaveSurveyResultsResult.Empty;
        }

        if (entries.Count == 0)
        {
            return SaveSurveyResultsResult.Empty;
        }

        await _dbContext.PanelistProfiles.AddRangeAsync(entries, cancellationToken);
        var pointsCollected = CalculateAwardedPoints(category, createdOn, lastSubmission);

        var reward = new UserReward
        {
            RewardId = Guid.NewGuid(),
            PanelistUlid = request.Ulid,
            CategoryId = request.CategoryId,
            Points = pointsCollected,
            TransactionType = RewardTransactionType.Earned,
            Description = BuildRewardDescription(category, lastSubmission, pointsCollected),
            CreatedOn = createdOn
        };

        await _dbContext.UserRewards.AddAsync(reward, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SaveSurveyResultsResult(pointsCollected);
    }

    private static string BuildRewardDescription(SurveyCategory category, DateTime? lastSubmission, int pointsCollected)
    {
        var categoryName = string.IsNullOrWhiteSpace(category.CategoryName)
            ? "Profile survey"
            : category.CategoryName.Trim();

        if (pointsCollected <= 0)
        {
            return $"{categoryName} survey submitted (no points awarded)";
        }

        var isRetake = lastSubmission.HasValue;
        return isRetake
            ? $"{categoryName} survey retake reward"
            : $"{categoryName} survey completion reward";
    }

    private static int CalculateAwardedPoints(SurveyCategory category, DateTime submissionOn, DateTime? lastSubmission)
    {
        if (category is null)
        {
            return 0;
        }

        if (!lastSubmission.HasValue)
        {
            return NormalizePoints(category.RewardPoints);
        }

        if (category.RetakePointsIssueFrequency <= 0)
        {
            return NormalizePoints(category.RetakePoints);
        }

        var nextEligibleDate = lastSubmission.Value.AddDays(category.RetakePointsIssueFrequency);

        if (submissionOn < nextEligibleDate)
        {
            return 0;
        }

        return NormalizePoints(category.RetakePoints);
    }

    private static int NormalizePoints(int points)
    {
        return points < 0 ? 0 : points;
    }

    private static List<PanelistProfile> ExtractEntries(
        string surveyJson,
        string panelistId,
        int categoryId,
        IReadOnlyDictionary<long, SurveyQuestion> questions,
        DateTime createdOn)
    {
        using var document = JsonDocument.Parse(surveyJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return new List<PanelistProfile>();
        }

        var entries = new List<PanelistProfile>();

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!long.TryParse(property.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var questionId))
            {
                continue;
            }

            if (!questions.TryGetValue(questionId, out var question))
            {
                continue;
            }

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    HandleObjectValue(property.Value, questionId, question, entries, panelistId, categoryId, createdOn);
                    break;
                case JsonValueKind.Array:
                    HandleArrayValue(property.Value, questionId, question, entries, panelistId, categoryId, createdOn);
                    break;
                default:
                    HandleScalarValue(property.Value, questionId, question, entries, panelistId, categoryId, createdOn);
                    break;
            }
        }

        return entries;
    }

    private static void HandleObjectValue(
        JsonElement value,
        long questionId,
        SurveyQuestion question,
        ICollection<PanelistProfile> entries,
        string panelistId,
        int categoryId,
        DateTime createdOn)
    {
        foreach (var matrixEntry in value.EnumerateObject())
        {
            var entry = CreateBaseEntry(panelistId, categoryId, questionId, createdOn);

            if (long.TryParse(matrixEntry.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var matrixId))
            {
                entry.MatrixQuestionId = matrixId;
            }

            PopulateEntryFromJsonValue(matrixEntry.Value, question, entry);
            entries.Add(entry);
        }
    }

    private static void HandleArrayValue(
        JsonElement value,
        long questionId,
        SurveyQuestion question,
        ICollection<PanelistProfile> entries,
        string panelistId,
        int categoryId,
        DateTime createdOn)
    {
        _ = question;

        var entry = CreateBaseEntry(panelistId, categoryId, questionId, createdOn);
        var builder = new StringBuilder();

        foreach (var item in value.EnumerateArray())
        {
            var representation = GetStringRepresentation(item);
            if (string.IsNullOrWhiteSpace(representation))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(',');
            }

            builder.Append(representation);
        }

        entry.AnswerIds = builder.Length > 0 ? builder.ToString() : null;
        entries.Add(entry);
    }

    private static void HandleScalarValue(
        JsonElement value,
        long questionId,
        SurveyQuestion question,
        ICollection<PanelistProfile> entries,
        string panelistId,
        int categoryId,
        DateTime createdOn)
    {
        var entry = CreateBaseEntry(panelistId, categoryId, questionId, createdOn);
        PopulateEntryFromJsonValue(value, question, entry);
        entries.Add(entry);
    }

    private static PanelistProfile CreateBaseEntry(string panelistId, int categoryId, long questionId, DateTime createdOn)
    {
        return new PanelistProfile
        {
            ResponseId = Guid.NewGuid(),
            PanelistId = panelistId,
            CategoryId = categoryId,
            QuestionId = questionId,
            CreatedOn = createdOn
        };
    }

    private static void PopulateEntryFromJsonValue(JsonElement value, SurveyQuestion question, PanelistProfile entry)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                var stringValue = value.GetString();
                if (IsTextResponse(question))
                {
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTimeValue))
                    {
                        entry.DateTime = dateTimeValue;
                    }
                    else
                    {
                        entry.Text = stringValue;
                    }
                }
                else
                {
                    entry.AnswerIds = stringValue;
                }

                break;
            case JsonValueKind.Number:
                if (value.TryGetDecimal(out var numericValue))
                {
                    entry.Numeric = numericValue;
                }
                else
                {
                    entry.AnswerIds = value.ToString();
                }

                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                entry.AnswerIds = value.GetBoolean() ? "true" : "false";
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
            default:
                entry.AnswerIds = GetStringRepresentation(value);
                break;
        }
    }

    private static bool IsTextResponse(SurveyQuestion question)
    {
        return string.Equals(question.ResponseType, "TEXT", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetStringRepresentation(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetDecimal(out var decimalValue)
                ? decimalValue.ToString(CultureInfo.InvariantCulture)
                : element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => element.ToString(),
            _ => element.ToString()
        };
    }
}
