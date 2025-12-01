using System.Threading;
using System.Threading.Tasks;
using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Common.Interfaces;

public interface IFirebaseAuthService
{
    Task<FirebaseSignUpResponse> SignUpAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<FirebaseSignInResponse> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<TokenVerificationResponse> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken = default);

    Task<FirebaseRefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
