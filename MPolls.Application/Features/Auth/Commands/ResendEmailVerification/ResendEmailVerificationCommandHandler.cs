using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Auth.Commands.ResendEmailVerification;

public sealed class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, Unit>
{
    private readonly IFirebaseAuthService _firebaseAuthService;

    public ResendEmailVerificationCommandHandler(IFirebaseAuthService firebaseAuthService)
    {
        _firebaseAuthService = firebaseAuthService;
    }

    public async Task<Unit> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        await _firebaseAuthService.SendEmailVerificationAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
