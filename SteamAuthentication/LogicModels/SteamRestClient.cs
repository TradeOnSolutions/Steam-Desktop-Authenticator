using System.Net;
using RestSharp;

namespace SteamAuthentication.LogicModels;

public class SteamRestClient
{
    private readonly RestClient _restClient;

    // ReSharper disable once MemberCanBeProtected.Global
    public SteamRestClient(HttpClient httpClient, IWebProxy? proxy)
    {
        _restClient = new RestClient(
            httpClient,
            new RestClientOptions
            {
                Proxy = proxy,
                FollowRedirects = true,
                AutomaticDecompression = DecompressionMethods.GZip,
            });
    }

    public async Task<RestResponse> ExecuteGetRequestAsync(string url, CookieContainer cookies,
        IEnumerable<(string name, string value)>? headers, string referer,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(url)
        {
            CookieContainer = cookies,
        };

        AddHeadersToRequest(request, referer);

        if (headers != null)
            foreach (var (name, value) in headers)
                request.AddHeader(name, value);

        var response = await _restClient.ExecuteAsync(request, cancellationToken);

        return response;
    }

    public async Task<RestResponse> ExecutePostRequestAsync(string url, CookieContainer cookies,
        IEnumerable<(string name, string value)>? headers, string referer,
        string body,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(url, Method.Post)
        {
            CookieContainer = cookies,
        };

        AddHeadersToRequest(request, referer);

        if (headers != null)
            foreach (var (name, value) in headers)
                request.AddHeader(name, value);

        request.AddBody(body, ContentType.FormUrlEncoded);

        var response = await _restClient.ExecuteAsync(request, cancellationToken);

        return response;
    }

    public async Task<RestResponse> ExecuteGetRequestAsync(string url, CookieContainer? cookies,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(url)
        {
            CookieContainer = cookies
        };

        AddHeadersToRequest(request);

        var response = await _restClient.ExecuteAsync(request, cancellationToken);

        return response;
    }

    protected async Task<RestResponse> ExecutePostRequestWithoutHeadersAsync(string url,
        CookieContainer? cookies,
        string body,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(url, Method.Post)
        {
            CookieContainer = cookies,
        };

        request.AddBody(body);

        var response = await _restClient.ExecuteAsync(request, cancellationToken);

        return response;
    }

    private static void AddHeadersToRequest(RestRequest request, string? referer = Endpoints.SteamCommunityUrl)
    {
        request.AddHeader("Accept", "application/json, text/javascript;q=0.9, */*;q=0.5");
        request.AddHeader("UserAgent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        request.AddHeader("Accept-Encoding", "gzip, deflate");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

        if (referer != null)
            request.AddHeader("Referer", referer);
    }
}