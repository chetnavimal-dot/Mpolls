using System.Text.Json.Serialization;

namespace MPolls.WebUI.Models;

public record ApiResponse<T>(
    [property: JsonPropertyName("statusCode")] int StatusCode,
    [property: JsonPropertyName("error")] ApiError? Error,
    [property: JsonPropertyName("payload")] T? Payload)
{
    public static ApiResponse<T> Success(T payload, int statusCode = 200)
        => new(statusCode, null, payload);

    public static ApiResponse<T> Failure(ApiError error, int statusCode, T? payload = default)
        => new(statusCode, error, payload);
}

public record ApiError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("details")] string? Details = null);
