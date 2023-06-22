using Newtonsoft.Json;

namespace TradeOnSda.Data;

public class MaFileCredentials
{
    [JsonProperty("ProxyString", Required = Required.Always)]
    public string? ProxyString { get; set; }

    [JsonProperty("SteamPassword", Required = Required.Always)]
    public string Password { get; set; }

    public MaFileCredentials(string? proxyString, string password)
    {
        ProxyString = proxyString;
        Password = password;
    }
}