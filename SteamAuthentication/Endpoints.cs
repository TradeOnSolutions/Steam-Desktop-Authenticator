namespace SteamAuthentication;

internal class Endpoints
{
    internal const string SteamCommunityUrl = "https://steamcommunity.com";

    internal const string SteamApiUrl = "https://api.steampowered.com";

    internal const string SteamIEconServiceBaseUrl = SteamApiUrl + "/IEconService/{0}/{1}/{2}";

    internal const string MobileAuthGetWgTokenUrl = SteamApiUrl + "/IMobileAuthService/GetWGToken/v0001";

    internal const string TwoFactorTimeQuery = SteamApiUrl + "/ITwoFactorService/QueryTime/v0001";
}