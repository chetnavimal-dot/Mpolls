using MediatR;
using MPolls.Application.DTOs.Surveys;
using MPolls.Domain.Enums;

namespace MPolls.Application.Features.ProfileQuestions.Queries.GetProfileQuestions;

public record GetProfileQuestionsQuery(ProfileQuestionCategory Category) : IRequest<SurveyJsSurveyDto>;
