using MediatR;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Queries.VerifyIdToken;

public record VerifyIdTokenQuery(string IdToken) : IRequest<TokenVerificationResponse>;
