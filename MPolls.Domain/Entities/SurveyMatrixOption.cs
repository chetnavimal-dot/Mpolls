namespace MPolls.Domain.Entities;

public class SurveyMatrixOption
{
    public long MatrixRowId { get; set; }

    public long QuestionId { get; set; }

    public string MatrixRowText { get; set; } = string.Empty;

    public SurveyQuestion Question { get; set; } = default!;
}
