namespace MPolls.Application.DTOs.Auth;

public record RefreshUserTokenResult(
    string IdToken,
    string RefreshToken,
    string ExpiresIn,
    string TokenType,
    string UserId,
    string ProjectId);
