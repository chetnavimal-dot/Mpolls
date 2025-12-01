using MediatR;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Queries.LoginUser;

public record LoginUserQuery(string Email, string Password) : IRequest<LoginUserResult>;
