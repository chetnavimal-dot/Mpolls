using System;
using System.Net;
using Microsoft.Extensions.Options;
using MPolls.API.Options;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;

namespace MPolls.API.Services;

public class FirebaseClientConfigProvider : IFirebaseClientConfigProvider
{
    private readonly FirebaseSettings _settings;

    public FirebaseClientConfigProvider(IOptions<FirebaseSettings> options)
    {
        _settings = options.Value;
    }

    public FirebaseClientConfigResponse GetClientConfig()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new FirebaseAuthException(
                "Firebase configuration is missing.",
                HttpStatusCode.InternalServerError,
                errorCode: "configuration_error");
        }

        return new FirebaseClientConfigResponse(
            _settings.ApiKey,
            _settings.AuthDomain,
            _settings.ProjectId,
            _settings.StorageBucket,
            _settings.MessagingSenderId,
            _settings.AppId);
    }
}
