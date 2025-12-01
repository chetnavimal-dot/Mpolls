using MediatR;
using MPolls.Application.DTOs.Profile;

namespace MPolls.Application.Features.ProfileQuestions.Queries.GetPanelistProfileDetails;

public sealed record GetPanelistProfileDetailsQuery(string PanelistId, int CategoryId) : IRequest<ProfileSurveyDetailsDto>;
