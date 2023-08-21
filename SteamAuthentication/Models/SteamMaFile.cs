using Newtonsoft.Json;

namespace SteamAuthentication.Models;

public class SteamMaFile
{
    [JsonProperty("shared_secret", Required = Required.Always)]
    public string SharedSecret { get; private set; } = null!;

    [JsonProperty("serial_number")]
    public string? SerialNumber { get; private set; }

    [JsonProperty("revocation_code")]
    public string? RevocationCode { get; private set; }

    [JsonProperty("uri")]
    public string? Uri { get; private set; }

    [JsonProperty("server_time")]
    public long ServerTime { get; private set; }

    [JsonProperty("account_name")]
    public string? AccountName { get; private set; }

    [JsonProperty("token_gid")]
    public string? TokenGuid { get; private set; }

    [JsonProperty("identity_secret", Required = Required.Always)]
    public string IdentitySecret { get; private set; } = null!;

    [JsonProperty("secret_1")]
    public string? Secret1 { get; private set; }

    [JsonProperty("status")]
    public int Status { get; private set; }

    [JsonProperty("device_id", Required = Required.Always)]
    public string DeviceId { get; private set; } = null!;

    [JsonProperty("fully_enrolled")]
    public bool FullyEnrolled { get; private set; }

    [JsonProperty("Session")]
    public SteamSessionData? Session { get; private set; } = null!;

    [JsonConstructor]
    private SteamMaFile()
    {
    }

    public string ConvertToJson() => JsonConvert.SerializeObject(this);

    public SteamMaFile(string sharedSecret, string? serialNumber, string revocationCode, string? uri, long serverTime,
        string accountName, string? tokenGuid, string identitySecret, string? secret1, int status, string deviceId,
        bool fullyEnrolled, SteamSessionData session)
    {
        SharedSecret = sharedSecret;
        SerialNumber = serialNumber;
        RevocationCode = revocationCode;
        Uri = uri;
        ServerTime = serverTime;
        AccountName = accountName;
        TokenGuid = tokenGuid;
        IdentitySecret = identitySecret;
        Secret1 = secret1;
        Status = status;
        DeviceId = deviceId;
        FullyEnrolled = fullyEnrolled;
        Session = session;
    }
}