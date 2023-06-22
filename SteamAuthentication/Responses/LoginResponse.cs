using Newtonsoft.Json;

namespace SteamAuthentication.Responses;

internal class LoginResponse
{
    [JsonProperty("success", Required = Required.Always)] public bool Success { get; set; }

    [JsonProperty("login_complete", Required = Required.Always)] public bool LoginComplete { get; set; }

    [JsonProperty("oauth")] public string? OAuthDataString { get; set; }

    [JsonIgnore]
    public OAuth? OAuthData =>
        OAuthDataString != null ? JsonConvert.DeserializeObject<OAuth>(OAuthDataString) : null;

    [JsonProperty("captcha_needed")] public bool CaptchaNeeded { get; set; }

    [JsonProperty("captcha_gid")] public string? CaptchaGuid { get; set; }

    [JsonProperty("emailsteamid")] public ulong EmailSteamId { get; set; }

    [JsonProperty("emailauth_needed")] public bool EmailAuthNeeded { get; set; }

    [JsonProperty("requires_twofactor")] public bool TwoFactorNeeded { get; set; }

    [JsonProperty("message")] public string? Message { get; set; }

    internal class OAuth
    {
        [JsonProperty("steamid", Required = Required.Always)] public ulong SteamId { get; set; }

        [JsonProperty("oauth_token")] public string? OAuthToken { get; set; }

        [JsonProperty("wgtoken", Required = Required.Always)] public string SteamLogin { get; set; } = null!;

        [JsonProperty("wgtoken_secure", Required = Required.Always)] public string SteamLoginSecure { get; set; } = null!;
    }
}