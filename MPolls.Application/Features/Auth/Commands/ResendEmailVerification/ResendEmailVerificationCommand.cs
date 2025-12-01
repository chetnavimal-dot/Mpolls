using MediatR;

namespace MPolls.Application.Features.Auth.Commands.ResendEmailVerification;

public sealed record ResendEmailVerificationCommand(string IdToken) : IRequest<Unit>;
