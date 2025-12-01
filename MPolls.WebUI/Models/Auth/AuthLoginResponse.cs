namespace MPolls.WebUI.Models.Auth;

public record AuthLoginResponse(
    string Kind,
    string LocalId,
    string Email,
    string DisplayName,
    bool Registered,
    string ExpiresIn,
    string Ulid,
    bool vFlag,
    bool IsOnboarded,
    int? Age,
    string? Gender,
    string? Country);
