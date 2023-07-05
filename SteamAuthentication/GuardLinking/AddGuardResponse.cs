using Newtonsoft.Json;
using SteamAuthentication.Trades.Responses.Converters;

namespace SteamAuthentication.GuardLinking;

#nullable disable

public class AddGuardResponseWrapper
{
    [JsonProperty("response", Required = Required.Always)]
    public AddGuardResponse Response { get; set; }
}

public class AddGuardResponse
{
    [JsonProperty("shared_secret")]
    public string SharedSecret { get; set; }

    [JsonProperty("serial_number")]
    public string SerialNumber { get; set; }

    [JsonProperty("revocation_code")]
    public string RevocationCode { get; set; }

    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("server_time")]
    [JsonConverter(typeof(ParseStringToLongConverter))]
    public long ServerTime { get; set; }

    [JsonProperty("account_name")]
    public string AccountName { get; set; }

    [JsonProperty("token_gid")]
    public string TokenGid { get; set; }

    [JsonProperty("identity_secret")]
    public string IdentitySecret { get; set; }

    [JsonProperty("secret_1")]
    public string Secret1 { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }
}