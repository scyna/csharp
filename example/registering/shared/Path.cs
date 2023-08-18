namespace ex.registering;

public class Path
{
    public const string REGISTRING = "registering";
    public const string CREATE_REGISTRATION = $"/{REGISTRING}/register";
    public const string RESEND_OTP = $"/{REGISTRING}/resend-otp";
    public const string VERIFY_REGISTRATION = $"/{REGISTRING}/verify";
    public const string ADAPTER = "adapter";
    public const string SEND_EMAIL = $"/{ADAPTER}/send-email";
}
