using Newtonsoft.Json;

namespace SteamAuthentication.Responses;

internal class RefreshSessionDataResponse
{
    [JsonProperty("response", Required = Required.Always)] internal RefreshSessionDataInternalResponse Response { get; set; }

    public RefreshSessionDataResponse(RefreshSessionDataInternalResponse response)
    {
        Response = response;
    }

    internal class RefreshSessionDataInternalResponse
    {
        [JsonProperty("token", Required = Required.Always)] public string Token { get; set; } = null!;

        [JsonProperty("token_secure", Required = Required.Always)] public string TokenSecure { get; set; } = null!;
    }
}