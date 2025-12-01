using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Surveys;
using MPolls.Domain.Entities;
using MPolls.Domain.Enums;

namespace MPolls.Application.Features.ProfileQuestions.Queries.GetProfileQuestions;

public class GetProfileQuestionsQueryHandler : IRequestHandler<GetProfileQuestionsQuery, SurveyJsSurveyDto>
{
    private static readonly IReadOnlyDictionary<ProfileQuestionCategory, string> CategoryLabels = new Dictionary<ProfileQuestionCategory, string>
    {
        [ProfileQuestionCategory.Automotive] = "Automotive",
        [ProfileQuestionCategory.Aviation] = "Aviation",
        [ProfileQuestionCategory.BusinessAndProfession] = "Business and Profession",
        [ProfileQuestionCategory.Cosmetics] = "Cosmetics",
        [ProfileQuestionCategory.Electronics] = "Electronics",
        [ProfileQuestionCategory.Entertainment] = "Entertainment",
        [ProfileQuestionCategory.Finance] = "Finance",
        [ProfileQuestionCategory.GeneralAndHousehold] = "General and Household",
        [ProfileQuestionCategory.Health] = "Health",
        [ProfileQuestionCategory.HomeAndGarden] = "Home & Garden",
        [ProfileQuestionCategory.InternetAndECommerce] = "Internet and eCommerce",
        [ProfileQuestionCategory.Leisure] = "Leisure",
        [ProfileQuestionCategory.Media] = "Media",
        [ProfileQuestionCategory.Pets] = "Pets",
        [ProfileQuestionCategory.Shopping] = "Shopping",
        [ProfileQuestionCategory.Travel] = "Travel"
    };

    private readonly ISurveyQuestionRepository _surveyQuestionRepository;

    public GetProfileQuestionsQueryHandler(ISurveyQuestionRepository surveyQuestionRepository)
    {
        _surveyQuestionRepository = surveyQuestionRepository;
    }

    public async Task<SurveyJsSurveyDto> Handle(GetProfileQuestionsQuery request, CancellationToken cancellationToken)
    {
        var questions = await _surveyQuestionRepository.GetByCategoryAsync((int)request.Category, cancellationToken);

        var survey = new SurveyJsSurveyDto
        {
            Title = $"{GetCategoryLabel(request.Category)} Profile Questions"
        };

        foreach (var question in questions)
        {
            var element = MapQuestionToElement(question);

            if (element is null)
            {
                continue;
            }

            survey.Pages.Add(new SurveyJsPageDto
            {
                Name = $"page_{question.QuestionId}",
                Elements = new List<SurveyJsElementDto> { element }
            });
        }

        return survey;
    }

    private static SurveyJsElementDto? MapQuestionToElement(SurveyQuestion question)
    {
        var element = new SurveyJsElementDto
        {
            Name = question.QuestionId.ToString(CultureInfo.InvariantCulture),
            Title = question.QuestionText?.Trim() ?? string.Empty,
            IsRequired = false
        };

        if (string.Equals(question.QuestionType, "Matrix", StringComparison.OrdinalIgnoreCase))
        {
            element.Type = "matrix";
            element.Columns = question.Options
                .OrderBy(option => option.OptionId)
                .Select(option => new SurveyJsMatrixColumnDto
                {
                    Name = option.OptionId.ToString(CultureInfo.InvariantCulture),
                    Value = option.OptionId.ToString(CultureInfo.InvariantCulture),
                    Text = option.OptionText?.Trim() ?? string.Empty
                })
                .ToList();
            element.Rows = question.MatrixOptions
                .OrderBy(option => option.MatrixRowId)
                .Select(option => new SurveyJsMatrixRowDto
                {
                    Name = option.MatrixRowId.ToString(CultureInfo.InvariantCulture),
                    Value = option.MatrixRowId.ToString(CultureInfo.InvariantCulture),
                    Text = option.MatrixRowText?.Trim() ?? string.Empty
                })
                .ToList();
            return element;
        }

        var responseType = question.ResponseType?.Trim().ToUpperInvariant();

        switch (responseType)
        {
            case "AND":
                element.Type = "checkbox";
                element.Choices = question.Options
                    .OrderBy(option => option.OptionId)
                    .Select(option => new SurveyJsChoiceDto
                    {
                        Name = option.OptionId.ToString(CultureInfo.InvariantCulture),
                        Value = option.OptionId.ToString(CultureInfo.InvariantCulture),
                        Text = option.OptionText?.Trim() ?? string.Empty
                    })
                    .ToList();
                break;
            case "TEXT":
                element.Type = "text";
                break;
            default:
                element.Type = "radiogroup";
                element.Choices = question.Options
                    .OrderBy(option => option.OptionId)
                    .Select(option => new SurveyJsChoiceDto
                    {
                        Name = option.OptionId.ToString(CultureInfo.InvariantCulture),
                        Value = option.OptionId.ToString(CultureInfo.InvariantCulture),
                        Text = option.OptionText?.Trim() ?? string.Empty
                    })
                    .ToList();
                break;
        }

        return element;
    }

    private static string GetCategoryLabel(ProfileQuestionCategory category)
    {
        return CategoryLabels.TryGetValue(category, out var label)
            ? label
            : category.ToString();
    }
}
