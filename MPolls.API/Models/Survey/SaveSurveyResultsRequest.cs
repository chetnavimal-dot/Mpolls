using System.ComponentModel.DataAnnotations;

namespace MPolls.API.Models.Survey;

public sealed class SaveSurveyResultsRequest
{
    [Required]
    public int CategoryId { get; init; }

    [Required]
    [MinLength(2)]
    public string SurveyJson { get; init; } = string.Empty;
}
