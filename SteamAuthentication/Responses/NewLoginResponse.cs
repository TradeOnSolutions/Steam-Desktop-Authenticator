using Newtonsoft.Json;

namespace SteamAuthentication.Responses;

public class NewLoginResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    [JsonProperty("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonProperty("transfer_urls")]
    public Uri[] TransferUrls { get; set; }

    [JsonProperty("transfer_parameters")]
    public TransferParameters TransferParameters { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
}

public class TransferParameters
{
    [JsonProperty("steamid")]
    public string SteamId { get; set; }

    [JsonProperty("token_secure")]
    public string TokenSecure { get; set; }

    [JsonProperty("auth")]
    public string Auth { get; set; }

    [JsonProperty("remember_login")]
    public bool RememberLogin { get; set; }

    [JsonProperty("webcookie")]
    public string Webcookie { get; set; }
}