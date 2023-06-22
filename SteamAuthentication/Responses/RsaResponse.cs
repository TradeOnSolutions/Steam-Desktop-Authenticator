using Newtonsoft.Json;

namespace SteamAuthentication.Responses;

internal class RsaResponse
{
    [JsonProperty("success", Required = Required.Always)] public bool Success { get; set; }

    [JsonProperty("publickey_exp", Required = Required.Always)] public string Exponent { get; set; } = null!;

    [JsonProperty("publickey_mod", Required = Required.Always)] public string Modulus { get; set; } = null!;

    [JsonProperty("timestamp", Required = Required.Always)] public string Timestamp { get; set; } = null!;
}