namespace scyna;

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class Utils
{
    public static string SubscribeURL(string urlPath)
    {
        //var subURL = rx.Replace(urlPath, "*");
        //subURL = subURL.Replace("/", ".");
        var subURL = urlPath.Replace("/", ".");
        return "API" + subURL;
    }

    public static String PublishURL(String urlPath)
    {
        var subURL = urlPath.Replace("/", ".");
        return "API" + subURL;
    }

    public static ulong CalculateID(uint prefix, ulong value)
    {
        return ((ulong)prefix << 44) + value;
    }

    public static long GetNanoseconds()
    {
        double timestamp = Stopwatch.GetTimestamp();
        double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

        return (long)nanoseconds;
    }
}
