using Newtonsoft.Json;

namespace SteamAuthentication.Responses;

public class ProcessConfirmationResponse
{
    [JsonProperty("success", Required = Required.Always)] public bool Success { get; set; }
}