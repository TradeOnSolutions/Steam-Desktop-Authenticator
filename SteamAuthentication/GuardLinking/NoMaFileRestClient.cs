using System.Net;
using RestSharp;

namespace SteamAuthentication.GuardLinking;

public class NoMaFileRestClient
{
    private readonly RestClient _rest;

    public NoMaFileRestClient(IWebProxy? proxy)
    {
        _rest = new RestClient(options => options.Proxy = proxy);
    }

    public async Task<RestResponse> SendPostAsync(string url, string query, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(url, Method.Post);

        request.AddHeader("User-Agent", "okhttp/3.12.12");

        request.AddBody(query, ContentType.FormUrlEncoded);
        
        var response = await _rest.ExecuteAsync(request, cancellationToken);

        return response;
    }
}