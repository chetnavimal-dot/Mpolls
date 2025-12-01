namespace MPolls.API.Models.Auth;

public record RefreshTokenResponse(
    string ExpiresIn,
    string TokenType,
    string UserId,
    string ProjectId);
