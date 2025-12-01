using System;

namespace MPolls.Domain.Entities;

public class PanelistProfile
{
    public Guid ResponseId { get; set; }

    public string PanelistId { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public long QuestionId { get; set; }

    public long? MatrixQuestionId { get; set; }

    public string? AnswerIds { get; set; }

    public string? Text { get; set; }

    public decimal? Numeric { get; set; }

    public DateTime? DateTime { get; set; }

    public DateTime CreatedOn { get; set; }

    public Panelist? Panelist { get; set; }
}
