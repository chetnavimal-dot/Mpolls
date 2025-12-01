namespace MPolls.Application.DTOs.Auth;

public record FirebaseRefreshTokenResponse(
    string IdToken,
    string RefreshToken,
    string ExpiresIn,
    string TokenType,
    string UserId,
    string ProjectId);
