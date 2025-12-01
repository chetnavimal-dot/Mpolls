namespace MPolls.WebUI.Models.Auth;

public record AuthRegistrationResponse(
    string Email,
    string ExpiresIn,
    string LocalId,
    string Ulid);
