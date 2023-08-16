namespace ex.registering;

public class Error
{
    public static scyna.Error OTP_EXPIRED = new(100, "Otp Expired");
    public static scyna.Error SMS_LIMIT_EXCEEDED = new(100, "Sms Limit Exceeded");
}
