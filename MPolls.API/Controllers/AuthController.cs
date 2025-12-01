using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MPolls.API.Models;
using MPolls.API.Models.Auth;
using MPolls.API.Services;
using MPolls.Application.Features.Auth.Commands.RefreshUserToken;
using MPolls.Application.Features.Auth.Commands.RegisterUser;
using MPolls.Application.Features.Auth.Commands.ResendEmailVerification;
using MPolls.Application.Features.Auth.Commands.SendPasswordResetEmail;
using MPolls.Application.Features.Auth.Queries.GetFirebaseClientConfig;
using MPolls.Application.Features.Auth.Queries.LoginUser;
using MPolls.Application.Features.Auth.Queries.VerifyIdToken;
using MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;


namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "mpolls_access_token";
    private const string RefreshTokenCookieName = "mpolls_refresh_token";
    private static readonly TimeSpan DefaultAccessTokenLifetime = TimeSpan.FromMinutes(55);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<ActionResult<ApiResponse<AuthRegistrationResponse>>> Register([FromBody] AuthRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return CreateErrorResponse<AuthRegistrationResponse>(
                StatusCodes.Status400BadRequest,
                "invalid_request",
                "Invalid request payload.");
        }

        try
        {
            var command = new RegisterUserCommand(request.Email, request.Password);
            var authResponse = await _mediator.Send(command, cancellationToken);

            SetAuthCookies(authResponse.IdToken, authResponse.RefreshToken, authResponse.ExpiresIn);

            var response = new AuthRegistrationResponse(authResponse.Email, authResponse.ExpiresIn, authResponse.LocalId, authResponse.Ulid);

            return Ok(ApiResponse<AuthRegistrationResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<AuthRegistrationResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<AuthRegistrationResponse>(ex, "An unexpected error occurred while registering.");
        }
    }

    [HttpPost("password-change")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<ActionResult<ApiResponse<PasswordChangeResponse>>> PasswordChange([FromBody] PasswordChangeRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
        {
            return CreateErrorResponse<PasswordChangeResponse>(StatusCodes.Status400BadRequest, "invalid_request", "Email is required to send the password reset email.");
        }

        var email = request.Email.Trim();

        try
        {
            var command = new SendPasswordResetEmailCommand(email);
            await _mediator.Send(command, cancellationToken);

            var response = new PasswordChangeResponse(
                "Password reset email sent successfully.",
                email);

            return Ok(ApiResponse<PasswordChangeResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<PasswordChangeResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<PasswordChangeResponse>(ex, "An unexpected error occurred while sending the password reset email.");
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<ActionResult<ApiResponse<AuthLoginResponse>>> Login([FromBody] AuthRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return CreateErrorResponse<AuthLoginResponse>(StatusCodes.Status400BadRequest,"invalid_request","Invalid request payload.");
        }

        try
        {
            var query = new LoginUserQuery(request.Email, request.Password);
            var authResponse = await _mediator.Send(query, cancellationToken);

            SetAuthCookies(authResponse.IdToken, authResponse.RefreshToken, authResponse.ExpiresIn);

            var response = new AuthLoginResponse(
                authResponse.Email,
                authResponse.DisplayName,
                authResponse.Registered,
                authResponse.ExpiresIn,
                authResponse.Ulid,
                authResponse.vFlag,
                authResponse.IsOnboarded,
                authResponse.Age,
                authResponse.Gender,
                authResponse.Country);

            return Ok(ApiResponse<AuthLoginResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<AuthLoginResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<AuthLoginResponse>(ex, "An unexpected error occurred while logging in.");
        }
    }

    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<ActionResult<ApiResponse<EmailVerificationResponse>>> ResendVerification([FromBody] EmailVerificationRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var idToken = request?.IdToken;
            if (string.IsNullOrWhiteSpace(idToken))
            {
                idToken = Request.Cookies[AccessTokenCookieName];
            }

            if (string.IsNullOrWhiteSpace(idToken))
            {
                return CreateErrorResponse<EmailVerificationResponse>(
                    StatusCodes.Status401Unauthorized,
                    "invalid_request",
                    "A valid session is required to send the verification email.");
            }

            idToken = idToken.Trim();

            var command = new ResendEmailVerificationCommand(idToken);
            await _mediator.Send(command, cancellationToken);

            var response = new EmailVerificationResponse("Verification email sent successfully.");

            return Ok(ApiResponse<EmailVerificationResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<EmailVerificationResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<EmailVerificationResponse>(ex, "An unexpected error occurred while sending the verification email.");
        }
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("AuthRateLimit")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh(CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return CreateErrorResponse<RefreshTokenResponse>(
                    StatusCodes.Status401Unauthorized,
                    "invalid_request",
                    "Refresh token is required to refresh authentication.");
            }

            refreshToken = refreshToken.Trim();

            var command = new RefreshUserTokenCommand(refreshToken);
            var refreshResult = await _mediator.Send(command, cancellationToken);

            var response = new RefreshTokenResponse(
                refreshResult.ExpiresIn,
                refreshResult.TokenType,
                refreshResult.UserId,
                refreshResult.ProjectId);

            SetAuthCookies(refreshResult.IdToken, refreshResult.RefreshToken, refreshResult.ExpiresIn);

            return Ok(ApiResponse<RefreshTokenResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            ClearAuthCookies();
            return CreateAuthProviderErrorResponse<RefreshTokenResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<RefreshTokenResponse>(ex, "An unexpected error occurred while refreshing authentication tokens.");
        }
    }

    [HttpPost("logout")]
    public ActionResult<ApiResponse<LogoutResponse>> Logout()
    {
        ClearAuthCookies();

        var response = new LogoutResponse("Signed out successfully.");
        return Ok(ApiResponse<LogoutResponse>.Success(response));
    }

    [HttpGet("session")]
    public async Task<ActionResult<ApiResponse<AuthLoginResponse>>> GetSession(CancellationToken cancellationToken)
    {
        var idToken = Request.Cookies[AccessTokenCookieName];

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return CreateErrorResponse<AuthLoginResponse>(
                StatusCodes.Status401Unauthorized,
                "unauthorized",
                "A valid session could not be found.");
        }

        try
        {
            idToken = idToken.Trim();

            var verification = await _mediator.Send(new VerifyIdTokenQuery(idToken), cancellationToken);
            var panelistSummary = await _mediator.Send(new GetPanelistSummaryQuery(verification.Uid), cancellationToken);

            if (panelistSummary is null)
            {
                return CreateErrorResponse<AuthLoginResponse>(
                    StatusCodes.Status404NotFound,
                    "panelist_not_found",
                    "Unable to locate the panelist for the current session.");
            }

            var email = GetStringClaim(verification.Claims, "email") ?? panelistSummary.Email;
            var displayName = GetStringClaim(verification.Claims, "name") ?? string.Empty;
            var registered = GetBoolClaim(verification.Claims, "email_verified");
            var expiresIn = GetRemainingLifetimeSeconds(idToken);

            var response = new AuthLoginResponse(
                email,
                displayName,
                registered,
                expiresIn,
                panelistSummary.Ulid,
                panelistSummary.Verified,
                panelistSummary.Onboarded,
                panelistSummary.Age,
                panelistSummary.Gender,
                panelistSummary.Country);

            return Ok(ApiResponse<AuthLoginResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            ClearAuthCookies();
            return CreateAuthProviderErrorResponse<AuthLoginResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<AuthLoginResponse>(ex, "An unexpected error occurred while retrieving the session.");
        }
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<AuthTokenVerificationResponse>>> VerifyToken([FromBody] TokenVerificationRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
        {
            return CreateErrorResponse<AuthTokenVerificationResponse>(
                StatusCodes.Status400BadRequest,
                "invalid_request",
                "ID token is required.");
        }

        try
        {
            var query = new VerifyIdTokenQuery(request.IdToken);
            var response = await _mediator.Send(query, cancellationToken);

            var apiResponse = new AuthTokenVerificationResponse(response.Uid, response.Claims);

            return Ok(ApiResponse<AuthTokenVerificationResponse>.Success(apiResponse));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<AuthTokenVerificationResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<AuthTokenVerificationResponse>(ex, "An unexpected error occurred while verifying the token.");
        }
    }

    [HttpGet("config")]
    public async Task<ActionResult<ApiResponse<AuthClientConfigResponse>>> GetConfig(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFirebaseClientConfigQuery();
            var config = await _mediator.Send(query, cancellationToken);
            var response = new AuthClientConfigResponse(
                config.ApiKey,
                config.AuthDomain,
                config.ProjectId,
                config.StorageBucket,
                config.MessagingSenderId,
                config.AppId);

            return Ok(ApiResponse<AuthClientConfigResponse>.Success(response));
        }
        catch (FirebaseAuthException ex)
        {
            return CreateAuthProviderErrorResponse<AuthClientConfigResponse>(ex);
        }
        catch (Exception ex)
        {
            return CreateUnexpectedErrorResponse<AuthClientConfigResponse>(ex, "An unexpected error occurred while retrieving the authentication configuration.");
        }
    }

    private void SetAuthCookies(string idToken, string refreshToken, string expiresIn)
    {
        if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var accessTokenLifetime = ParseAccessTokenLifetime(expiresIn);
        var accessTokenOptions = CreateCookieOptions(accessTokenLifetime);

        Response.Cookies.Append(AccessTokenCookieName, idToken, accessTokenOptions);

        var refreshTokenOptions = CreateCookieOptions(RefreshTokenLifetime);
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshTokenOptions);
    }

    private static CookieOptions CreateCookieOptions(TimeSpan lifetime)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            IsEssential = true,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(lifetime)
        };
    }

    private static TimeSpan ParseAccessTokenLifetime(string expiresIn)
    {
        if (double.TryParse(expiresIn, NumberStyles.Number, CultureInfo.InvariantCulture, out var seconds) && seconds > 0)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        return DefaultAccessTokenLifetime;
    }

    private void ClearAuthCookies()
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        Response.Cookies.Delete(AccessTokenCookieName, options);
        Response.Cookies.Delete(RefreshTokenCookieName, options);
    }

    private static string? GetStringClaim(IReadOnlyDictionary<string, object?> claims, string key)
    {
        if (claims.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }

        return null;
    }

    private static bool GetBoolClaim(IReadOnlyDictionary<string, object?> claims, string key)
    {
        if (claims.TryGetValue(key, out var value))
        {
            return value switch
            {
                bool boolean => boolean,
                string str when bool.TryParse(str, out var parsed) => parsed,
                _ => false
            };
        }

        return false;
    }

    private static string GetRemainingLifetimeSeconds(string idToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(idToken))
            {
                return ((int)DefaultAccessTokenLifetime.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            var token = handler.ReadJwtToken(idToken);
            var exp = token.Payload.Exp;

            if (!exp.HasValue)
            {
                return ((int)DefaultAccessTokenLifetime.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            var expiration = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp.Value));
            var remaining = expiration - DateTimeOffset.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                return "0";
            }

            var seconds = (int)Math.Floor(remaining.TotalSeconds);
            return seconds.ToString(CultureInfo.InvariantCulture);
        }
        catch
        {
            return ((int)DefaultAccessTokenLifetime.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
    }

    private ObjectResult CreateAuthProviderErrorResponse<T>(FirebaseAuthException exception)
    {
        var statusCode = (int)exception.StatusCode;
        return CreateErrorResponse<T>(
            statusCode,
            exception.ErrorCode ?? "auth_provider_error",
            exception.Message,
            exception.Details);
    }

    private ObjectResult CreateUnexpectedErrorResponse<T>(Exception exception, string message)
    {
        return CreateErrorResponse<T>(
            StatusCodes.Status500InternalServerError,
            "unexpected_error",
            message,
            exception.Message);
    }

    private ObjectResult CreateErrorResponse<T>(int statusCode, string code, string message, string? details = null, T? payload = default)
    {
        var error = new ApiError(code, message, details);
        var response = ApiResponse<T>.Failure(error, statusCode, payload);
        return StatusCode(statusCode, response);
    }
}
