namespace MPolls.Application.DTOs.Auth;

public record FirebaseSignUpResponse(
    string IdToken,
    string Email,
    string RefreshToken,
    string ExpiresIn,
    string LocalId);
