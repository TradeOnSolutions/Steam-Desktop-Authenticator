using System.Net;
using Newtonsoft.Json;

namespace SteamAuthentication.Logic;

public static class ExtensionUtilities
{
    public static string ToJson(this Exception exception) => JsonConvert.SerializeObject(exception);

    public static string ToJson(this CookieContainer cookieContainer)
    {
        var cookies = cookieContainer.GetAllCookies().Cast<Cookie>();

        var json = JsonConvert.SerializeObject(cookies);

        return json;
    }

    public static string ToQuery(this Dictionary<string, string> queryParameters) =>
        string.Join("&", queryParameters.Select(t => $"{WebUtility.UrlEncode(t.Key)}={WebUtility.UrlEncode(t.Value)}"));
}