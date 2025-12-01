using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MPolls.WebUI.Models.Survey;

public class SurveyJsSurveyModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("showProgressBar")]
    public string? ShowProgressBar { get; set; }

    [JsonPropertyName("questionsOnPageMode")]
    public string? QuestionsOnPageMode { get; set; }

    [JsonPropertyName("pages")]
    public List<SurveyJsPageModel> Pages { get; set; } = new();

    [JsonIgnore]
    public bool HasQuestions => Pages.Any(page => page.Elements.Count > 0);
}

public class SurveyJsPageModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("elements")]
    public List<SurveyJsElementModel> Elements { get; set; } = new();
}

public class SurveyJsElementModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<SurveyJsChoiceModel>? Choices { get; set; }

    [JsonPropertyName("columns")]
    public List<SurveyJsMatrixColumnModel>? Columns { get; set; }

    [JsonPropertyName("rows")]
    public List<SurveyJsMatrixRowModel>? Rows { get; set; }

    [JsonPropertyName("inputType")]
    public string? InputType { get; set; }

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }
}

public class SurveyJsChoiceModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class SurveyJsMatrixColumnModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class SurveyJsMatrixRowModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
