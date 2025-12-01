using MediatR;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Queries.GetFirebaseClientConfig;

public record GetFirebaseClientConfigQuery : IRequest<FirebaseClientConfigResponse>;
