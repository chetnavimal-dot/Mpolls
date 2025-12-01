using System;
using System.Collections.Generic;

namespace MPolls.WebUI.Models.Survey;

public sealed class ProfileSurveyDetailsModel
{
    public int CategoryId { get; set; }

    public int TotalQuestionCount { get; set; }

    public DateTime? LastResponseOn { get; set; }

    public IReadOnlyList<ProfileQuestionDetailModel> Responses { get; set; } = new List<ProfileQuestionDetailModel>();
}
