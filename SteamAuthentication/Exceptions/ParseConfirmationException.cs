namespace SteamAuthentication.Exceptions;

public class ParseConfirmationException : Exception
{
    public string Content { get; }

    public ParseConfirmationException(string content)
    {
        Content = content;
    }
}