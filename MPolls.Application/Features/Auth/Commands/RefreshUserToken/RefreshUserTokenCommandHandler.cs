using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Features.Auth.Commands.RefreshUserToken;

public sealed class RefreshUserTokenCommandHandler : IRequestHandler<RefreshUserTokenCommand, RefreshUserTokenResult>
{
    private readonly IFirebaseAuthService _firebaseAuthService;

    public RefreshUserTokenCommandHandler(IFirebaseAuthService firebaseAuthService)
    {
        _firebaseAuthService = firebaseAuthService;
    }

    public async Task<RefreshUserTokenResult> Handle(RefreshUserTokenCommand request, CancellationToken cancellationToken)
    {
        var response = await _firebaseAuthService.RefreshTokenAsync(request.RefreshToken, cancellationToken).ConfigureAwait(false);

        return new RefreshUserTokenResult(
            response.IdToken,
            response.RefreshToken,
            response.ExpiresIn,
            response.TokenType,
            response.UserId,
            response.ProjectId);
    }
}
