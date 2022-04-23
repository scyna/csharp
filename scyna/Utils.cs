namespace scyna
{
    class Utils
    {
        private const string PATH_REGEX = ":[A-z,0-9,$,-,_,.,+,!,*,',(,),\\,]{1,}";

        public static string SubscribeURL(string urlPath)
        {
            // String subURL = urlPath.Replace() PATH_REGEX, "*");
            // subURL = subURL.replaceAll("/", ".");
            // subURL = String.format("API%s", subURL);
            return urlPath;
        }

        public static String PublishURL(String urlPath)
        {
            // var subURL = urlPath.replaceAll("/", ".");
            // subURL = String.format("API%s", subURL);
            return urlPath;
        }

        public static ulong CalculateID(uint prefix, ulong value)
        {
            return ((ulong)prefix << 44) + value;
        }
    }
}