namespace SteamAuthentication.GuardLinking;

public class PhoneNumberException : Exception
{
    public string? Email { get; set; }

    public PhoneNumberException(string? email)
    {
        Email = email;
    }
}