namespace MPolls.Domain.Entities;

public class SurveyOption
{
    public long OptionId { get; set; }

    public long QuestionId { get; set; }

    public string OptionText { get; set; } = string.Empty;

    public SurveyQuestion Question { get; set; } = default!;
}
