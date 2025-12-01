namespace MPolls.Application.DTOs.Auth;

public record RegisterUserResult(
    string IdToken,
    string Email,
    string RefreshToken,
    string ExpiresIn,
    string LocalId,
    string Ulid);
