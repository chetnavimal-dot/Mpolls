using MediatR;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Commands.RefreshUserToken;

public sealed record RefreshUserTokenCommand(string RefreshToken) : IRequest<RefreshUserTokenResult>;
