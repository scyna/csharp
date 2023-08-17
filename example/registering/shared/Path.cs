namespace ex.registering;

public class Path
{
    public static string REGISTRING = "registering";
    public static string CREATE_REGISTRATION = $"/{REGISTRING}/register";
    public static string RESEND_OTP = $"/{REGISTRING}/resend-otp";
    public static string VERIFY_REGISTRATION = $"/{REGISTRING}/verify";

    public static string ADAPTER = "adapter";
    public static string SEND_EMAIL = $"/{ADAPTER}/send-email";
}
