using MPolls.Application.DTOs.Auth;

namespace MPolls.Application.Common.Interfaces;

public interface IFirebaseClientConfigProvider
{
    FirebaseClientConfigResponse GetClientConfig();
}
