using System.Net;
using Newtonsoft.Json;
using SteamAuthentication.Exceptions;

#pragma warning disable CS0168

namespace SteamAuthentication.LogicModels;

public class TimeDeferenceRestClient : SteamRestClient
{
    public TimeDeferenceRestClient(IWebProxy? proxy) : base(proxy)
    {
    }

    public async Task<long> GetSteamTimeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response =
                await ExecutePostRequestWithoutHeadersAsync(Endpoints.TwoFactorTimeQuery, null, "steamid=0", cancellationToken);

            if (!response.IsSuccessful)
                throw new RequestException("Response is not successful", response.StatusCode, response.Content, null);

            if (response.Content == null)
                throw new RequestException("Response content is null", response.StatusCode, null, null);

            var timeQuery = JsonConvert.DeserializeObject<TimeQuery>(response.Content);

            if (timeQuery == null)
                throw new RequestException("Deserialized time query value is null", response.StatusCode,
                    response.Content, null);

            return timeQuery.Response.ServerTime;
        }
        catch (RequestException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RequestException("Exception while getting time difference", null, null, e);
        }
    }
}

internal class TimeQuery
{
    [JsonProperty("response")] internal TimeQueryResponse Response { get; }

    public TimeQuery(TimeQueryResponse response)
    {
        Response = response;
    }

    internal class TimeQueryResponse
    {
        [JsonProperty("server_time")] public long ServerTime { get; }

        public TimeQueryResponse(long serverTime)
        {
            ServerTime = serverTime;
        }
    }
}