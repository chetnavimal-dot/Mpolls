using Microsoft.AspNetCore.Http;

namespace MPolls.API.Models;

public record ApiResponse<T>(int StatusCode, ApiError? Error, T? Payload)
{
    public static ApiResponse<T> Success(T payload, int statusCode = StatusCodes.Status200OK)
        => new(statusCode, null, payload);

    public static ApiResponse<T> Failure(ApiError error, int statusCode, T? payload = default)
        => new(statusCode, error, payload);
}

public record ApiError(string Code, string Message, string? Details = null);
