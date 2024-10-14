using Newtonsoft.Json;
using RestSharp;
using SteamKit2.Discovery;

namespace SteamAuthentication.LogicModels;

public class SteamServersProvider : IServerListProvider
{
    private const string ServerListUrl = "https://api.steampowered.com/ISteamDirectory/GetCMListForConnect/v1/?cmtype=websockets";

    public async Task<IEnumerable<ServerRecord>> FetchServerListAsync()
    {
        using var restClient = new RestClient();

        var request = new RestRequest(ServerListUrl);

        var response = await restClient.ExecuteAsync(request);

        var result = JsonConvert.DeserializeObject<ResponseWrapper>(response.Content!)!;

        return result.Response.Servers.Select(t => ServerRecord.CreateWebSocketServer(t.Endpoint));
    }

    public Task UpdateServerListAsync(IEnumerable<ServerRecord> endpoints) => Task.CompletedTask;
}

file class ResponseWrapper
{
    [JsonProperty("response")]
    public required Response Response { get; init; }
}

file class Response
{
    [JsonProperty("serverlist")]
    public required ServerResponse[] Servers { get; init; }
}

file class ServerResponse
{
    [JsonProperty("endpoint")]
    public required string Endpoint { get; init; }
}