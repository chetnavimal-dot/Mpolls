namespace MPolls.API.Models.Auth;

public record AuthClientConfigResponse(
    string ApiKey,
    string AuthDomain,
    string ProjectId,
    string StorageBucket,
    string MessagingSenderId,
    string AppId);
