namespace SteamAuthentication.Models;

public enum LoginResult
{
    LoginOkay,
    GeneralFailure,
    BadRsa,
    BadCredentials,
    NeedCaptcha,
    Need2Fa,
    NeedEmail,
    TooManyFailedLogins,
    UnknownError,
}