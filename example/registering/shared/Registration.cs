using scyna;

namespace ex.registering;

public class Registration
{
    private static bool testing = false;
    public static void Setup()
    {
        Endpoint.Register(Path.CREATE_REGISTRATION, new CreateRegistrationHandler());
        Endpoint.Register(Path.VERIFY_REGISTRATION, new VerifyRegistrationHandler());
        Endpoint.Register(Path.RESEND_OTP, new ResendOtpHandler());

        if (testing) return;

        DomainEvent.Register(new SendOtpHandler());
        DomainEvent.Register(new WelcomeHandler());
        DomainEvent.Register(new WaitSixMinutesHandler());
    }

    public static void TestingSetup()
    {
        testing = true;
        Setup();
    }
}