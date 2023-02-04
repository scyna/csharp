namespace scyna;

public class Utils
{
    // private const string PATH_REGEX = ":[A-z,0-9,$,-,_,.,+,!,*,',(,),\\,]{1,}";
    // private static Regex rx = new Regex(":[A-z,0-9,$,-,_,.,+,!,*,',(,),\\,]{1,}", RegexOptions.Compiled);

    public static string SubscribeURL(string urlPath)
    {
        // var subURL = rx.Replace(urlPath, "*");
        // subURL = subURL.Replace("/", ".");
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
}
