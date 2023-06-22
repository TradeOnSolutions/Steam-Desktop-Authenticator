using System.Net;

namespace SteamAuthentication.Exceptions;

public class RequestException : Exception
{
    public HttpStatusCode? HttpStatusCode { get; }

    public string? Content { get; }

    public RequestException(string message, HttpStatusCode? httpStatusCode, string? content, Exception? innerException)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
        Content = content;
    }
}