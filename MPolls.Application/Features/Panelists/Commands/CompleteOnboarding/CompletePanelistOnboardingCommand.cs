using MediatR;

namespace MPolls.Application.Features.Panelists.Commands.CompleteOnboarding;

public sealed record CompletePanelistOnboardingCommand(
    string FirebaseId,
    int Age,
    int Gender,
    int CountryCode) : IRequest<bool>;
