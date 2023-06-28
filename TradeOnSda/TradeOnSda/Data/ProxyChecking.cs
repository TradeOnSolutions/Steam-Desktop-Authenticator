using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace TradeOnSda.Data;

public static class ProxyChecking
{
    public static async Task<bool> CheckProxyAsync(IWebProxy proxy)
    {
        var client = new RestClient(options =>
        {
            options.Proxy = proxy;
            options.MaxTimeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
        });

        return await CheckProxyAsync(client);
    }

    private static async Task<bool> CheckProxyAsync(IRestClient client)
    {
        try
        {
            var request = new RestRequest("https://gstatic.com/generate_204");

            var response = await client.ExecuteAsync(request);

            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception)
        {
            return false;
        }
    }
}