namespace MPolls.Application.DTOs.Auth;

public record FirebaseSignInResponse(
    string Kind,
    string LocalId,
    string Email,
    string DisplayName,
    string IdToken,
    bool Registered,
    string RefreshToken,
    string ExpiresIn);
