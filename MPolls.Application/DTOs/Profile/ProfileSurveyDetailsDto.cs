using System;
using System.Collections.Generic;

namespace MPolls.Application.DTOs.Profile;

public sealed class ProfileSurveyDetailsDto
{
    public int CategoryId { get; init; }

    public int TotalQuestionCount { get; init; }

    public DateTime? LastResponseOn { get; init; }

    public IReadOnlyList<ProfileQuestionDetailDto> Responses { get; init; } = new List<ProfileQuestionDetailDto>();
}
