using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using MPolls.API.Options;
using MPolls.Application.DTOs.Auth;
using IFirebaseAuthService = MPolls.Application.Common.Interfaces.IFirebaseAuthService;


namespace MPolls.API.Services;

public class FirebaseAuthService : IFirebaseAuthService
{
    private const string IdentityToolkitBaseUrl = "https://identitytoolkit.googleapis.com/v1/";
    private const string SecureTokenBaseUrl = "https://securetoken.googleapis.com/v1/";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly Dictionary<string, string> FirebaseErrorMessages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EMAIL_EXISTS"] = "The email address is already registered. Please sign in instead.",
        ["EMAIL_NOT_FOUND"] = "There is no user record for the provided credentials.",
        ["INVALID_EMAIL"] = "The email address is badly formatted.",
        ["INVALID_PASSWORD"] = "The password is invalid or the user does not have a password.",
        ["INVALID_LOGIN_CREDENTIALS"] = "The email or password is incorrect.",
        ["MISSING_EMAIL"] = "An email address is required to continue.",
        ["MISSING_PASSWORD"] = "A password is required to continue.",
        ["OPERATION_NOT_ALLOWED"] = "Password sign-in is disabled for this project.",
        ["TOO_MANY_ATTEMPTS_TRY_LATER"] = "We have blocked all requests from this device due to unusual activity. Try again later.",
        ["USER_DISABLED"] = "The user account has been disabled by an administrator.",
        ["USER_NOT_FOUND"] = "There is no user record for the provided credentials.",
        ["WEAK_PASSWORD"] = "The password is too weak.",
        ["INVALID_REFRESH_TOKEN"] = "The refresh token is invalid or has expired.",
    };
    private static FirebaseAuth? _adminAuth;
    private static readonly object AdminAuthLock = new();
    private readonly HttpClient _httpClient;
    private readonly FirebaseSettings _settings;

    public FirebaseAuthService(HttpClient httpClient, IOptions<FirebaseSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<FirebaseSignUpResponse> SignUpAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var firebaseResponse = await ExecuteAsync("accounts:signUp", email, password, cancellationToken).ConfigureAwait(false);
        return MapToSignUpResponse(firebaseResponse);
    }

    public async Task<FirebaseSignInResponse> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var firebaseResponse = await ExecuteAsync("accounts:signInWithPassword", email, password, cancellationToken).ConfigureAwait(false);
        return MapToSignInResponse(firebaseResponse);
    }

    public async Task<TokenVerificationResponse> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new FirebaseAuthException("ID token must be provided.", HttpStatusCode.BadRequest, errorCode: "invalid_id_token");
        }

        var adminAuth = EnsureAdminAuth();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var decodedToken = await adminAuth.VerifyIdTokenAsync(idToken).ConfigureAwait(false);
            var claims = decodedToken.Claims.ToDictionary(pair => pair.Key, pair => (object?)pair.Value);

            return new TokenVerificationResponse(decodedToken.Uid, claims);
        }
        catch (FirebaseAuthException)
        {
            throw;
        }
        catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
        {
            var statusCode = ex.AuthErrorCode switch
            {
                AuthErrorCode.ExpiredIdToken or AuthErrorCode.RevokedIdToken => HttpStatusCode.Unauthorized,
                AuthErrorCode.InvalidIdToken => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError,
            };

            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? "Firebase token verification failed."
                : ex.Message;

            throw new FirebaseAuthException(
                message,
                statusCode,
                ex,
                errorCode: ex.AuthErrorCode.ToString(),
                details: ex.InnerException?.Message);
        }
        catch (ArgumentException ex)
        {
            throw new FirebaseAuthException(ex.Message, HttpStatusCode.BadRequest, ex, errorCode: "invalid_argument", details: ex.ParamName);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new FirebaseAuthException(
                "Email must be provided to send a password reset email.",
                HttpStatusCode.BadRequest,
                errorCode: "invalid_email");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new FirebaseAuthException(
                "Firebase API key is not configured.",
                HttpStatusCode.InternalServerError,
                errorCode: "configuration_error");
        }

        var requestUri = new Uri($"{IdentityToolkitBaseUrl}accounts:sendOobCode?key={_settings.ApiKey}");
        var request = new FirebasePasswordResetRequest(email.Trim());

        using var response = await _httpClient.PostAsJsonAsync(
            requestUri,
            request,
            SerializerOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var errorResponse = await JsonSerializer.DeserializeAsync<FirebaseErrorResponse>(
            contentStream,
            SerializerOptions,
            cancellationToken);

        var errorInfo = ParseFirebaseError(errorResponse);

        throw new FirebaseAuthException(
            errorInfo.Message,
            response.StatusCode,
            errorCode: errorInfo.Code,
            details: errorInfo.Details);
    }

    public async Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new FirebaseAuthException(
                "An ID token must be provided to send a verification email.",
                HttpStatusCode.BadRequest,
                errorCode: "invalid_id_token");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new FirebaseAuthException(
                "Firebase API key is not configured.",
                HttpStatusCode.InternalServerError,
                errorCode: "configuration_error");
        }

        var requestUri = new Uri($"{IdentityToolkitBaseUrl}accounts:sendOobCode?key={_settings.ApiKey}");

        var trimmedIdToken = idToken.Trim();
        var request = new FirebaseEmailVerificationRequest(trimmedIdToken);

        using var response = await _httpClient.PostAsJsonAsync(
            requestUri,
            request,
            SerializerOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var errorResponse = await JsonSerializer.DeserializeAsync<FirebaseErrorResponse>(
            contentStream,
            SerializerOptions,
            cancellationToken);

        var errorInfo = ParseFirebaseError(errorResponse);

        throw new FirebaseAuthException(
            errorInfo.Message,
            response.StatusCode,
            errorCode: errorInfo.Code,
            details: errorInfo.Details);
    }

    public async Task<FirebaseRefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new FirebaseAuthException(
                "A refresh token must be provided to obtain a new ID token.",
                HttpStatusCode.BadRequest,
                errorCode: "invalid_refresh_token");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new FirebaseAuthException(
                "Firebase API key is not configured.",
                HttpStatusCode.InternalServerError,
                errorCode: "configuration_error");
        }

        var requestUri = new Uri($"{SecureTokenBaseUrl}token?key={_settings.ApiKey}");
        var trimmedRefreshToken = refreshToken.Trim();

        if (string.IsNullOrWhiteSpace(trimmedRefreshToken))
        {
            throw new FirebaseAuthException(
                "A refresh token must be provided to obtain a new ID token.",
                HttpStatusCode.BadRequest,
                errorCode: "invalid_refresh_token");
        }

        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", trimmedRefreshToken),
        });

        using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var firebaseResponse = await JsonSerializer.DeserializeAsync<FirebaseSecureTokenResponse>(
                contentStream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            if (firebaseResponse is null || string.IsNullOrWhiteSpace(firebaseResponse.IdToken))
            {
                throw new FirebaseAuthException(
                    "Unexpected response from Firebase.",
                    HttpStatusCode.InternalServerError,
                    errorCode: "unexpected_firebase_response");
            }

            return new FirebaseRefreshTokenResponse(
                firebaseResponse.IdToken,
                firebaseResponse.RefreshToken ?? string.Empty,
                firebaseResponse.ExpiresIn ?? string.Empty,
                firebaseResponse.TokenType ?? string.Empty,
                firebaseResponse.UserId ?? string.Empty,
                firebaseResponse.ProjectId ?? string.Empty);
        }

        var errorResponse = await JsonSerializer.DeserializeAsync<FirebaseErrorResponse>(
            contentStream,
            SerializerOptions,
            cancellationToken).ConfigureAwait(false);

        var errorInfo = ParseFirebaseError(errorResponse);

        throw new FirebaseAuthException(
            errorInfo.Message,
            response.StatusCode,
            errorCode: errorInfo.Code,
            details: errorInfo.Details);
    }

    private async Task<FirebaseAuthResponse> ExecuteAsync(string endpoint, string email, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new FirebaseAuthException(
                "Firebase API key is not configured.",
                HttpStatusCode.InternalServerError,
                errorCode: "configuration_error");
        }

        var request = new FirebaseEmailPasswordRequest(email, password);
        var requestUri = new Uri($"{IdentityToolkitBaseUrl}{endpoint}?key={_settings.ApiKey}");

        var response = await _httpClient.PostAsJsonAsync(
            requestUri,
            request,
            SerializerOptions,
            cancellationToken);

        return await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static FirebaseSignUpResponse MapToSignUpResponse(FirebaseAuthResponse firebaseResponse)
    {
        if (string.IsNullOrWhiteSpace(firebaseResponse.IdToken))
        {
            throw new FirebaseAuthException(
                "Unexpected response from Firebase.",
                HttpStatusCode.InternalServerError,
                errorCode: "unexpected_firebase_response");
        }

        return new FirebaseSignUpResponse(
            firebaseResponse.IdToken,
            firebaseResponse.Email ?? string.Empty,
            firebaseResponse.RefreshToken ?? string.Empty,
            firebaseResponse.ExpiresIn ?? string.Empty,
            firebaseResponse.LocalId ?? string.Empty);
    }

    private static FirebaseSignInResponse MapToSignInResponse(FirebaseAuthResponse firebaseResponse)
    {
        if (string.IsNullOrWhiteSpace(firebaseResponse.IdToken))
        {
            throw new FirebaseAuthException(
                "Unexpected response from Firebase.",
                HttpStatusCode.InternalServerError,
                errorCode: "unexpected_firebase_response");
        }

        return new FirebaseSignInResponse(
            firebaseResponse.Kind ?? string.Empty,
            firebaseResponse.LocalId ?? string.Empty,
            firebaseResponse.Email ?? string.Empty,
            firebaseResponse.DisplayName ?? string.Empty,
            firebaseResponse.IdToken,
            firebaseResponse.Registered ?? false,
            firebaseResponse.RefreshToken ?? string.Empty,
            firebaseResponse.ExpiresIn ?? string.Empty);
    }

    private static async Task<FirebaseAuthResponse> HandleResponseAsync(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        await using var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);

        if (httpResponse.IsSuccessStatusCode)
        {
            var firebaseResponse = await JsonSerializer.DeserializeAsync<FirebaseAuthResponse>(
                contentStream,
                SerializerOptions,
                cancellationToken);

            if (firebaseResponse is null)
            {
                throw new FirebaseAuthException(
                    "Unexpected response from Firebase.",
                    HttpStatusCode.InternalServerError,
                    errorCode: "unexpected_firebase_response");
            }

            return firebaseResponse;
        }

        var errorResponse = await JsonSerializer.DeserializeAsync<FirebaseErrorResponse>(
            contentStream,
            SerializerOptions,
            cancellationToken);

        var errorInfo = ParseFirebaseError(errorResponse);
        throw new FirebaseAuthException(
            errorInfo.Message,
            httpResponse.StatusCode,
            errorCode: errorInfo.Code,
            details: errorInfo.Details);
    }

    private FirebaseAuth EnsureAdminAuth()
    {
        if (_adminAuth is not null)
        {
            return _adminAuth;
        }

        lock (AdminAuthLock)
        {
            if (_adminAuth is not null)
            {
                return _adminAuth;
            }

            FirebaseApp? app = TryGetDefaultApp();

            if (app is null)
            {
                if (string.IsNullOrWhiteSpace(_settings.AdminSdkCredentialPath))
                {
                    throw new FirebaseAuthException(
                        "Firebase Admin SDK credential path is not configured.",
                        HttpStatusCode.InternalServerError,
                        errorCode: "configuration_error");
                }

                if (!File.Exists(_settings.AdminSdkCredentialPath))
                {
                    throw new FirebaseAuthException(
                        $"Firebase Admin SDK credential file was not found at '{_settings.AdminSdkCredentialPath}'.",
                        HttpStatusCode.InternalServerError,
                        errorCode: "configuration_error");
                }

                var options = new AppOptions
                {
                    Credential = GoogleCredential.FromFile(_settings.AdminSdkCredentialPath),
                };

                if (!string.IsNullOrWhiteSpace(_settings.ProjectId))
                {
                    options.ProjectId = _settings.ProjectId;
                }

                app = FirebaseApp.Create(options);
            }

            _adminAuth = FirebaseAdmin.Auth.FirebaseAuth.GetAuth(app);
            return _adminAuth;
        }
    }

    private static FirebaseErrorInfo ParseFirebaseError(FirebaseErrorResponse? errorResponse)
    {
        var rawMessage = errorResponse?.Error?.Message;

        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return new FirebaseErrorInfo("firebase_error", "Firebase authentication request failed.", null);
        }

        var trimmedMessage = rawMessage.Trim();
        string? details = null;

        var separatorIndex = trimmedMessage.IndexOf(':');
        if (separatorIndex >= 0)
        {
            details = trimmedMessage[(separatorIndex + 1)..].Trim();
            trimmedMessage = trimmedMessage[..separatorIndex].Trim();
        }

        var code = trimmedMessage.Replace(' ', '_').ToUpperInvariant();

        string message;
        if (FirebaseErrorMessages.TryGetValue(code, out var friendlyMessage))
        {
            message = string.IsNullOrWhiteSpace(details)
                ? friendlyMessage
                : $"{friendlyMessage} {details}".Trim();
        }
        else
        {
            message = FormatDefaultFirebaseMessage(code, details);
        }

        return new FirebaseErrorInfo(code, message, details);
    }

    private static string FormatDefaultFirebaseMessage(string code, string? details)
    {
        if (!string.IsNullOrWhiteSpace(details))
        {
            return details;
        }

        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        var spacedCode = code.Replace('_', ' ').ToLowerInvariant();
        return textInfo.ToTitleCase(spacedCode);
    }

    private readonly record struct FirebaseErrorInfo(string Code, string Message, string? Details);

    private static FirebaseApp? TryGetDefaultApp()
    {
        try
        {
            return FirebaseApp.DefaultInstance;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private sealed record FirebaseEmailPasswordRequest
    {
        public FirebaseEmailPasswordRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }

        [JsonPropertyName("email")]
        public string Email { get; }

        [JsonPropertyName("password")]
        public string Password { get; }

        [JsonPropertyName("returnSecureToken")]
        public bool ReturnSecureToken => true;
    }

    private sealed record FirebasePasswordResetRequest
    {
        public FirebasePasswordResetRequest(string email)
        {
            Email = email;
        }

        [JsonPropertyName("requestType")]
        public string RequestType => "PASSWORD_RESET";

        [JsonPropertyName("email")]
        public string Email { get; }
    }

    private sealed record FirebaseEmailVerificationRequest
    {
        public FirebaseEmailVerificationRequest(string idToken)
        {
            IdToken = idToken;
        }

        [JsonPropertyName("requestType")]
        public string RequestType => "VERIFY_EMAIL";

        [JsonPropertyName("idToken")]
        public string IdToken { get; }

    }

    private sealed class FirebaseAuthResponse
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("idToken")]
        public string? IdToken { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expiresIn")]
        public string? ExpiresIn { get; set; }

        [JsonPropertyName("localId")]
        public string? LocalId { get; set; }

        [JsonPropertyName("registered")]
        public bool? Registered { get; set; }
    }

    private sealed class FirebaseSecureTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public string? ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("project_id")]
        public string? ProjectId { get; set; }
    }

    private sealed class FirebaseErrorResponse
    {
        [JsonPropertyName("error")]
        public FirebaseErrorDetail? Error { get; set; }
    }

    private sealed class FirebaseErrorDetail
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
