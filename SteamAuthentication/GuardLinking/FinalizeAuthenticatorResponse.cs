using Newtonsoft.Json;

namespace SteamAuthentication.GuardLinking;

#nullable disable

public class FinalizeAuthenticatorResponseWrapper
{
    [JsonProperty("response")]
    public FinalizeAuthenticatorResponse Response { get; set; }
}

public class FinalizeAuthenticatorResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("want_more")]
    public bool WantMore { get; set; }

    [JsonProperty("server_time")]
    public long ServerTime { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }
}