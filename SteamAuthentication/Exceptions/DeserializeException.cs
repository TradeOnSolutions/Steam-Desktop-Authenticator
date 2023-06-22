namespace SteamAuthentication.Exceptions;

public class DeserializeException : Exception
{
    public DeserializeException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}