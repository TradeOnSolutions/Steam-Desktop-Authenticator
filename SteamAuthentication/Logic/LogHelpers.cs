using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace SteamAuthentication.Logic;

public static class LogHelpers
{
    public static void LogRestResponse(this ILogger logger, RestResponse response)
    {
        logger.LogDebug("Response got, statusCode: {statusCode}, rawBytesCount: {rawBytesCount}",
            response.StatusCode,
            response.RawBytes?.Length);
    }

    public static void LogRestResponse(this ILogger logger, RestResponse response, string info)
    {
        logger.LogDebug("Response got, info: {info}, statusCode: {statusCode}, rawBytesCount: {rawBytesCount}",
            info,
            response.StatusCode,
            response.RawBytes?.Length);
    }

    public static IDisposable? CreateScopeForMethod(this ILogger logger, object model,
        [CallerMemberName] string? methodName = null)
    {
        return logger.BeginScope("Model: {model}. Method: {methodName}", model, methodName);
    }
}