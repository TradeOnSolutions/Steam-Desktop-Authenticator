using System;
using System.Net;

namespace TradeOnSda.Data;

public static class ProxyLogic
{
    public static IWebProxy? ParseWebProxy(string? proxyString)
    {
        if (string.IsNullOrWhiteSpace(proxyString))
            return null;

        var protocol = "http://";

        if (proxyString.ToLower().StartsWith("https://"))
            proxyString = proxyString[8..];
        else if (proxyString.ToLower().StartsWith("http://")) proxyString = proxyString[7..];

        var tokens = proxyString.Split(':');

        return tokens.Length switch
        {
            2 => new WebProxy(protocol + tokens[0], int.Parse(tokens[1]))
            {
                UseDefaultCredentials = false,
                BypassProxyOnLocal = false,
            },
            4 => new WebProxy(protocol + tokens[0], int.Parse(tokens[1]))
            {
                UseDefaultCredentials = false,
                BypassProxyOnLocal = false,
                Credentials = new NetworkCredential(tokens[2], tokens[3]),
            },
            _ => throw new Exception("Invalid proxy format")
        };
    }
}