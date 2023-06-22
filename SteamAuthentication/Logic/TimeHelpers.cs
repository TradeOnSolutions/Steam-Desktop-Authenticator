namespace SteamAuthentication.Logic;

public static class TimeHelpers
{
    public static long ToTimeStamp(this DateTime dateTime) => 
        (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    
    public static DateTime FromTimeStamp(long timestamp) => 
        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
}