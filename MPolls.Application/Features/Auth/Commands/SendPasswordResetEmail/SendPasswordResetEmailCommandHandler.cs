using MediatR;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Auth.Commands.SendPasswordResetEmail;

public sealed class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, Unit>
{
    private readonly IFirebaseAuthService _firebaseAuthService;

    public SendPasswordResetEmailCommandHandler(IFirebaseAuthService firebaseAuthService)
    {
        _firebaseAuthService = firebaseAuthService;
    }

    public async Task<Unit> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        await _firebaseAuthService.SendPasswordResetEmailAsync(request.Email, cancellationToken);
        return Unit.Value;
    }
}
