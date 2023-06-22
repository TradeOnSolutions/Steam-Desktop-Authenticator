using Newtonsoft.Json;

namespace SteamAuthentication.Models;

public class SteamMaFile
{
    [JsonProperty("shared_secret", Required = Required.Always)]
    public string SharedSecret { get; private set; } = null!;

    [JsonProperty("serial_number", Required = Required.Always)]
    public string SerialNumber { get; private set; } = null!;

    [JsonProperty("revocation_code", Required = Required.Always)]
    public string RevocationCode { get; private set; } = null!;

    [JsonProperty("uri", Required = Required.Always)]
    public string Uri { get; private set; } = null!;

    [JsonProperty("server_time", Required = Required.Always)]
    public long ServerTime { get; private set; }

    [JsonProperty("account_name", Required = Required.Always)]
    public string AccountName { get; private set; } = null!;

    [JsonProperty("token_gid", Required = Required.Always)]
    public string TokenGuid { get; private set; } = null!;

    [JsonProperty("identity_secret", Required = Required.Always)]
    public string IdentitySecret { get; private set; } = null!;

    [JsonProperty("secret_1", Required = Required.Always)]
    public string Secret1 { get; private set; } = null!;

    [JsonProperty("status", Required = Required.Always)]
    public int Status { get; private set; }

    [JsonProperty("device_id", Required = Required.Always)]
    public string DeviceId { get; private set; } = null!;

    [JsonProperty("fully_enrolled", Required = Required.Always)]
    public bool FullyEnrolled { get; private set; }

    [JsonProperty("Session", Required = Required.Always)]
    public SteamSessionData Session { get; private set; } = null!;

    [JsonConstructor]
    private SteamMaFile()
    {
    }

    public string ConvertToJson() => JsonConvert.SerializeObject(this);

    public SteamMaFile(string sharedSecret, string serialNumber, string revocationCode, string uri, long serverTime,
        string accountName, string tokenGuid, string identitySecret, string secret1, int status, string deviceId,
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