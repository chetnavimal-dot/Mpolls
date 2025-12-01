using System.Net;

namespace MPolls.API.Services;

public class FirebaseAuthException : Exception
{
    public FirebaseAuthException(string message, HttpStatusCode statusCode, string? errorCode = null, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }

    public FirebaseAuthException(string message, HttpStatusCode statusCode, Exception? innerException, string? errorCode = null, string? details = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ErrorCode { get; }

    public string? Details { get; }
}
