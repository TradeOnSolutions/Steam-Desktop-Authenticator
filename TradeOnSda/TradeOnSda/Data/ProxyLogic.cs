using System;
using System.Net;

namespace TradeOnSda.Data;

public static class ProxyLogic
{
    public static IWebProxy? ParseWebProxy(string? proxyString)
    {
        if (string.IsNullOrWhiteSpace(proxyString))
            return null;

        var tokens = proxyString.Split(':');

        return tokens.Length switch
        {
            2 => new WebProxy(tokens[0], int.Parse(tokens[1])),
            4 => new WebProxy(tokens[0], int.Parse(tokens[1]))
            {
                Credentials = new NetworkCredential(tokens[2], tokens[3]),
            },
            _ => throw new Exception("Invalid proxy format")
        };
    }
}