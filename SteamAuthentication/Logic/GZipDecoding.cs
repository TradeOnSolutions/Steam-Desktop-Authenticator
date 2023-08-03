using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SteamAuthentication.Logic;

public static class GZipDecoding
{
    public static async Task<string> DecodeGZipAsync(byte[] bytes, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Start decoding gzip");

        var gZipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
        
        var stringReader = new StreamReader(gZipStream);

        try
        {
            var content = await stringReader.ReadToEndAsync(cancellationToken);

            return content;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}