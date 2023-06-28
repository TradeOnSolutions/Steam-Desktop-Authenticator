using System.Net;

namespace TradeOnSda.Data;

public class MaFileCredentials
{
    public IWebProxy? Proxy { get; set; }
    
    public string? ProxyString { get; set; }
    
    public string Password { get; set; }

    public MaFileCredentials(IWebProxy? proxy, string? proxyString, string password)
    {
        Proxy = proxy;
        ProxyString = proxyString;
        Password = password;
    }
}