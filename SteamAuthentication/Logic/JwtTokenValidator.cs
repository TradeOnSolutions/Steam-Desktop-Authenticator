using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamAuthentication.Logic;

public static class JwtTokenValidator
{
    public static bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));

        var tokenParts = token.Split('.');
        if (tokenParts.Length != 3)
            throw new ArgumentException("Token is not a valid JWT token.");

        var payload = tokenParts[1];

        var jsonBytes = ParseBase64WithoutPadding(payload);
        var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);

        var payloadData = JsonSerializer.Deserialize<JwtPayload>(jsonString);

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(payloadData.Exp).UtcDateTime;
        return expirationTime < DateTime.UtcNow;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }

    private class JwtPayload
    {
        [JsonPropertyName("exp")]
        public long Exp { get; set; }
    }
}
