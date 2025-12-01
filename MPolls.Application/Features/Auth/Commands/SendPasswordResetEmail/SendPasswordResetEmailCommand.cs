using MediatR;

namespace MPolls.Application.Features.Auth.Commands.SendPasswordResetEmail;

public sealed record SendPasswordResetEmailCommand(string Email) : IRequest<Unit>;
