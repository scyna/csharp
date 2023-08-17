namespace ex.registering;

public class Utils
{
    public static string GenerateSixDigitsOtp()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}