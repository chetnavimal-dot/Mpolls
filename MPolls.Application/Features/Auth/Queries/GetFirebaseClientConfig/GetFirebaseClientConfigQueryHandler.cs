using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Queries.GetFirebaseClientConfig;

public class GetFirebaseClientConfigQueryHandler : IRequestHandler<GetFirebaseClientConfigQuery, FirebaseClientConfigResponse>
{
    private readonly IFirebaseClientConfigProvider _configProvider;

    public GetFirebaseClientConfigQueryHandler(IFirebaseClientConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public Task<FirebaseClientConfigResponse> Handle(GetFirebaseClientConfigQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_configProvider.GetClientConfig());
    }
}
