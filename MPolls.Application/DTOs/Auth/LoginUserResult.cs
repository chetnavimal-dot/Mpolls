namespace MPolls.Application.DTOs.Auth;

public record LoginUserResult(
    string Kind,
    string LocalId,
    string Email,
    string DisplayName,
    string IdToken,
    bool Registered,
    string RefreshToken,
    string ExpiresIn,
    string Ulid,
    bool vFlag,
    bool IsOnboarded,
    int? Age,
    string? Gender,
    string? Country);
