using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MPolls.Application.DTOs.Surveys;

public class SurveyJsSurveyDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("showProgressBar")]
    public string ShowProgressBar { get; set; } = "top";

    [JsonPropertyName("questionsOnPageMode")]
    public string QuestionsOnPageMode { get; set; } = "questionPerPage";

    [JsonPropertyName("pages")]
    public List<SurveyJsPageDto> Pages { get; set; } = new();
}

public class SurveyJsPageDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("elements")]
    public List<SurveyJsElementDto> Elements { get; set; } = new();
}

public class SurveyJsElementDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("choices")]
    public List<SurveyJsChoiceDto>? Choices { get; set; }

    [JsonPropertyName("columns")]
    public List<SurveyJsMatrixColumnDto>? Columns { get; set; }

    [JsonPropertyName("rows")]
    public List<SurveyJsMatrixRowDto>? Rows { get; set; }

    [JsonPropertyName("inputType")]
    public string? InputType { get; set; }

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; } = false;
}

public class SurveyJsChoiceDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class SurveyJsMatrixColumnDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class SurveyJsMatrixRowDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
