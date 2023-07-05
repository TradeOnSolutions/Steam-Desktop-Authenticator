using Newtonsoft.Json;

namespace SteamAuthentication.GuardLinking;

#nullable disable

public class GetUserCountryResponseWrapper
{
    [JsonProperty("response")]
    public GetUserCountryResponse Response { get; set; }
}

public class GetUserCountryResponse
{
    [JsonProperty("country")]
    public string Country { get; set; }
}