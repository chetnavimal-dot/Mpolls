using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Auth;

namespace MPolls.WebUI.Services;

/// <summary>
/// Wraps HTTP calls for authentication endpoints so the UI can drive the session lifecycle.
/// </summary>
public class AuthClient
{
    private readonly HttpClient _httpClient;

    public AuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Attempts to sign in and returns the authenticated user payload on success.
    /// </summary>
    public Task<ApiResponse<AuthLoginResponse>> LoginAsync(
        AuthRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<AuthLoginResponse>("api/v1/Auth/login", request, cancellationToken);

    /// <summary>
    /// Registers a new account and returns the created profile information.
    /// </summary>
    public Task<ApiResponse<AuthRegistrationResponse>> RegisterAsync(
        AuthRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<AuthRegistrationResponse>("api/v1/Auth/register", request, cancellationToken);

    /// <summary>
    /// Sends a password reset email for the provided account.
    /// </summary>
    public Task<ApiResponse<PasswordChangeResponse>> SendPasswordResetEmailAsync(
        PasswordChangeRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<PasswordChangeResponse>("api/v1/Auth/password-change", request, cancellationToken);

    /// <summary>
    /// Resends the verification email if the user has not confirmed their address yet.
    /// </summary>
    public Task<ApiResponse<EmailVerificationResponse>> ResendEmailVerificationAsync(
        EmailVerificationRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<EmailVerificationResponse>("api/v1/Auth/resend-verification", request, cancellationToken);

    /// <summary>
    /// Retrieves the current session payload to determine whether the user is authenticated.
    /// </summary>
    public Task<ApiResponse<AuthLoginResponse>> GetSessionAsync(CancellationToken cancellationToken = default)
        => GetAsync<AuthLoginResponse>("api/v1/Auth/session", cancellationToken);

    /// <summary>
    /// Exchanges a refresh token for a new session when the current one expires.
    /// </summary>
    public Task<ApiResponse<RefreshTokenResponse>> RefreshSessionAsync(CancellationToken cancellationToken = default)
        => PostAsync<RefreshTokenResponse>("api/v1/Auth/refresh", null, cancellationToken);

    /// <summary>
    /// Revokes the server-side session and clears cookies.
    /// </summary>
    public Task<ApiResponse<LogoutResponse>> LogoutAsync(CancellationToken cancellationToken = default)
        => PostAsync<LogoutResponse>("api/v1/Auth/logout", null, cancellationToken);

    private async Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        try
        {
            var apiResponse = await _httpClient.GetFromJsonAsync<ApiResponse<T>>(url, cancellationToken: cancellationToken);

            if (apiResponse is not null)
            {
                return apiResponse;
            }

            var error = new ApiError("deserialization_error", "Unable to parse the server response.");
            return ApiResponse<T>.Failure(error, (int)HttpStatusCode.InternalServerError);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            var error = new ApiError(
                "network_error",
                "We couldn't reach the MPolls servers. Please check your connection and try again.");
            return ApiResponse<T>.Failure(error, (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception)
        {
            var error = new ApiError(
                "client_error",
                "An unexpected error occurred while processing the request. Please try again.");
            return ApiResponse<T>.Failure(error, (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string url, object? payload, CancellationToken cancellationToken)
    {
        try
        {
            using var response = payload is null
                ? await _httpClient.PostAsync(url, null, cancellationToken)
                : await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);

            if (apiResponse is not null)
            {
                return apiResponse;
            }

            var statusCode = (int)response.StatusCode;
            var error = new ApiError("deserialization_error", "Unable to parse the server response.");
            return ApiResponse<T>.Failure(error, statusCode);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            var error = new ApiError(
                "network_error",
                "We couldn't reach the MPolls servers. Please check your connection and try again.");
            return ApiResponse<T>.Failure(error, (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception)
        {
            var error = new ApiError(
                "client_error",
                "An unexpected error occurred while processing the request. Please try again.");
            return ApiResponse<T>.Failure(error, (int)HttpStatusCode.InternalServerError);
        }
    }
}
