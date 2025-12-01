using System.Collections.Generic;

namespace MPolls.WebUI.Models.Survey;

public sealed class ProfileQuestionDetailModel
{
    public long QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public IReadOnlyList<ProfileAnswerDetailModel> Answers { get; set; } = new List<ProfileAnswerDetailModel>();
}
