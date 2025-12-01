namespace MPolls.API.Models.Auth;

public record AuthRegistrationResponse(
    string Email,
    string ExpiresIn,
    string LocalId,
    string Ulid);
