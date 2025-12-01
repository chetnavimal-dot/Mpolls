using MediatR;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password) : IRequest<RegisterUserResult>;
