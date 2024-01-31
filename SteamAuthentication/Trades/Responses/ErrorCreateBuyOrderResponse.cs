using Newtonsoft.Json;

namespace SteamAuthentication.Trades.Responses;

internal class ErrorCreateBuyOrderResponse
{
    [JsonProperty("success")] public int ErrorCode { get; set; }
}