using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SteamAuthentication.Logic;

internal static class SteamGuardCodeGenerating
{
    private static readonly byte[] SteamGuardCodeTranslations = "23456789BCDFGHJKMNPQRTVWXY"u8.ToArray();

    internal static string? GenerateSteamGuardCode(string sharedSecret, long timestamp, ILogger logger)
    {
        if (string.IsNullOrEmpty(sharedSecret))
            return "";

        var sharedSecretUnescaped = Regex.Unescape(sharedSecret);
        var sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);

        var timeArray = new byte[8];

        timestamp /= 30L;

        for (var i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)timestamp;
            timestamp >>= 8;
        }

        var hmacGenerator = new HMACSHA1();
        hmacGenerator.Key = sharedSecretArray;

        var hashedData = hmacGenerator.ComputeHash(timeArray);
        var codeArray = new byte[5];

        try
        {
            var b = (byte)(hashedData[19] & 0xF);

            var codePoint =
                (hashedData[b] & 0x7F) << 24 |
                (hashedData[b + 1] & 0xFF) << 16 |
                (hashedData[b + 2] & 0xFF) << 8 |
                (hashedData[b + 3] & 0xFF);

            for (var i = 0; i < 5; ++i)
            {
                codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                codePoint /= SteamGuardCodeTranslations.Length;
            }
        }
        catch (Exception e)
        {
            logger.LogError("Error compute sda code, exception: {exception}", e.ToJson());
            return null;
        }

        return Encoding.UTF8.GetString(codeArray);
    }

    public static string? GenerateConfirmationHash(long timeStamp, string? tag, string identitySecret, ILogger logger)
    {
        var decode = Convert.FromBase64String(identitySecret);
        var n2 = 8;

        if (tag != null)
            if (tag.Length > 32)
                n2 = 8 + 32;
            else
                n2 = 8 + tag.Length;

        var array = new byte[n2];
        var n3 = 8;

        while (true)
        {
            var n4 = n3 - 1;
            
            if (n3 <= 0)
                break;

            array[n4] = (byte)timeStamp;
            timeStamp >>= 8;
            n3 = n4;
        }

        if (tag != null) 
            Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);

        try
        {
            var hmacGenerator = new HMACSHA1();
            
            hmacGenerator.Key = decode;
            
            var hashedData = hmacGenerator.ComputeHash(array);
            var encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
            var hash = WebUtility.UrlEncode(encodedData);

            return hash;
        }
        catch (Exception e)
        {
            logger.LogError("Error compute confirmation hash, exception: {exception}", e.ToJson());
            
            return null;
        }
    }
}