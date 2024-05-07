using System.Net;
using Newtonsoft.Json;

namespace SteamAuthentication.Models;

public class SteamSessionData
{
    [JsonProperty("SessionID")]
    public string? SessionId { get; private set; }

    // [JsonProperty("SteamLogin", Required = Required.Always)]
    // public string SteamLogin { get; private set; } = null!;

    [JsonProperty("SteamLoginSecure")]
    public string? SteamLoginSecure { get; private set; }

    // [JsonProperty("OAuthToken", Required = Required.Always)]
    // public string OAuthToken { get; private set; } = null!;

    [JsonProperty("RefreshToken")]
    public string? RefreshToken { get; private set; }

    [JsonProperty("SteamID", Required = Required.Always)]
    public ulong SteamId { get; private set; }

    [JsonConstructor]
    private SteamSessionData()
    {
    }

    internal SteamSessionData(string sessionId, string steamLoginSecure,
        string refreshToken, ulong steamId)
    {
        SessionId = sessionId;
        SteamLoginSecure = steamLoginSecure;
        // OAuthToken = oAuthToken;
        RefreshToken = refreshToken;
        SteamId = steamId;
    }

    public void SetCookies(CookieContainer container)
    {
        // container.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
        // container.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));
        container.Add(new Cookie("steamid", SteamId.ToString(), "/", ".steamcommunity.com"));

        // container.Add(new Cookie("steamLogin", SteamLogin, "/", ".steamcommunity.com")
        // {
        //     HttpOnly = true
        // });

        container.Add(new Cookie("steamLoginSecure", SteamLoginSecure, "/", ".steamcommunity.com")
        {
            HttpOnly = true,
            Secure = true
        });

        container.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
        container.Add(new Cookie("dob", "", "/", ".steamcommunity.com"));
        container.Add(new Cookie("sessionid", SessionId, "/", ".steamcommunity.com"));
    }

    public CookieContainer CreateCookies()
    {
        var container = new CookieContainer();

        SetCookies(container);

        return container;
    }
}