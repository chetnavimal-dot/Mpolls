using System.Collections.Generic;

namespace MPolls.Application.DTOs.Profile;

public sealed class ProfileQuestionDetailDto
{
    public long QuestionId { get; init; }

    public string QuestionText { get; init; } = string.Empty;

    public IReadOnlyList<ProfileAnswerDetailDto> Answers { get; init; } = new List<ProfileAnswerDetailDto>();
}
