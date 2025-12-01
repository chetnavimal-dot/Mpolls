using System.Collections.Generic;

namespace MPolls.Domain.Entities;

public class SurveyQuestion
{
    public long QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string ResponseType { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public string? CountryCode { get; set; }

    public ICollection<SurveyOption> Options { get; set; } = new List<SurveyOption>();

    public ICollection<SurveyMatrixOption> MatrixOptions { get; set; } = new List<SurveyMatrixOption>();
}
