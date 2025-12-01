namespace MPolls.Application.DTOs.Auth;

public record FirebaseClientConfigResponse(
    string ApiKey,
    string AuthDomain,
    string ProjectId,
    string StorageBucket,
    string MessagingSenderId,
    string AppId);
