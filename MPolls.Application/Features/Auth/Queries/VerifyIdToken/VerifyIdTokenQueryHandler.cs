using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Queries.VerifyIdToken;

public class VerifyIdTokenQueryHandler : IRequestHandler<VerifyIdTokenQuery, TokenVerificationResponse>
{
    private readonly IFirebaseAuthService _firebaseAuthService;

    public VerifyIdTokenQueryHandler(IFirebaseAuthService firebaseAuthService)
    {
        _firebaseAuthService = firebaseAuthService;
    }

    public Task<TokenVerificationResponse> Handle(VerifyIdTokenQuery request, CancellationToken cancellationToken)
    {
        return _firebaseAuthService.VerifyIdTokenAsync(request.IdToken, cancellationToken);
    }
}
