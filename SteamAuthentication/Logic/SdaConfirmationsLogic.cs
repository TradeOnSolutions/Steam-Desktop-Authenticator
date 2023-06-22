using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SteamAuthentication.Exceptions;
using SteamAuthentication.LogicModels;
using SteamAuthentication.Models;

namespace SteamAuthentication.Logic;

internal static partial class SdaConfirmationsLogic
{
    // public static SdaConfirmation[] ParseConfirmations(string content, ILogger<SteamGuardAccount> logger)
    // {
    //     logger.LogDebug("start parsing confirmations");
    //
    //     var confRegex = ConfirmationsRegex();
    //
    //     if (!confRegex.IsMatch(content))
    //     {
    //         if (!content.Contains("<div>Nothing to confirm</div>"))
    //         {
    //             logger.LogError("Error. Unknown format of confirmations html");
    //             throw new ParseConfirmationException(content);
    //         }
    //
    //         logger.LogDebug("Parse result: nothing to confirm");
    //         return Array.Empty<SdaConfirmation>();
    //     }
    //
    //     var matches = confRegex.Matches(content);
    //
    //     var confirmations = new List<SdaConfirmation>();
    //
    //     foreach (Match confirmation in matches)
    //     {
    //         if (confirmation.Groups.Count != 5) continue;
    //
    //         if (!ulong.TryParse(confirmation.Groups[1].Value, out var confirmationId) ||
    //             !ulong.TryParse(confirmation.Groups[2].Value, out var confirmationKey) ||
    //             !int.TryParse(confirmation.Groups[3].Value, out var confirmationType) ||
    //             !ulong.TryParse(confirmation.Groups[4].Value, out var confirmationCreator))
    //             continue;
    //
    //         logger.LogDebug("Added confirmation, confirmationId: {confirmationId}", confirmationId);
    //         confirmations.Add(new SdaConfirmation(confirmationId, confirmationKey, confirmationCreator,
    //             confirmationType));
    //     }
    //
    //     return confirmations.ToArray();
    // }
    //
    // [GeneratedRegex(
    //     "<div class=\"mobileconf_list_entry\" id=\"conf[0-9]+\" data-confid=\"(\\d+)\" data-key=\"(\\d+)\" data-type=\"(\\d+)\" data-creator=\"(\\d+)\"")]
    // private static partial Regex ConfirmationsRegex();

    public static string GenerateConfirmationUrl(long timeStamp, string deviceId, string identitySecret,
        ulong steamId, string tag, ILogger logger)
    {
        var endpoint = Endpoints.SteamCommunityUrl + "/mobileconf/getlist?";

        var queryString = GenerateConfirmationQueryParams(tag, deviceId, identitySecret, steamId, timeStamp, logger);

        return endpoint + queryString;
    }

    public static string GenerateConfirmationQueryParams(string tag, string deviceId, string identitySecret,
        ulong steamId, long timeStamp, ILogger logger)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentException("Device Id is not present");

        var queryParams =
            GenerateConfirmationQueryParameters(tag, deviceId, identitySecret, steamId, timeStamp, logger);

        return "p=" + queryParams["p"] + "&a=" + queryParams["a"] + "&k=" + queryParams["k"] + "&t=" +
               queryParams["t"] + "&m=android&tag=" + queryParams["tag"];
    }

    public static NameValueCollection GenerateConfirmationQueryParameters(string tag, string deviceId,
        string identitySecret,
        ulong steamId,
        long timeStamp,
        ILogger logger)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentException("Device Id is not present");

        var k = SteamGuardCodeGenerating.GenerateConfirmationHash(timeStamp, tag, identitySecret, logger);

        if (k == null)
            throw new ArgumentException("Cannot generate confirmation hash");

        var result = new NameValueCollection
        {
            { "p", deviceId },
            { "a", steamId.ToString() },
            { "k", k },
            { "t", timeStamp.ToString() },
            { "m", "android" },
            { "tag", tag }
        };

        return result;
    }
}