using Newtonsoft.Json;

namespace SteamAuthentication.GuardLinking;

#nullable disable

public class SetPhoneNumberResponseWrapper
{
    [JsonProperty("response")]
    public SetPhoneNumberResponse Response { get; set; }
}

public class SetPhoneNumberResponse
{
    [JsonProperty("confirmation_email_address")]
    public string ConfirmationEmailAddress { get; set; }

    [JsonProperty("phone_number_formatted")]
    public string PhoneNumberFormatted { get; set; }
}