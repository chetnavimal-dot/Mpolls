using MediatR;

namespace MPolls.Application.Features.ProfileQuestions.Commands.SaveSurveyResults;

public sealed record SaveSurveyResultsCommand(string Ulid, int CategoryId, string SurveyJson)
    : IRequest<SaveSurveyResultsResult>;
