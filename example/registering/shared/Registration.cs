using scyna;

namespace ex.registering;

class Registration
{
    public static void Setup()
    {
        Endpoint.Register(Path.CREATE_REGISTRATION, new CreateRegistrationHandler());
    }

    public static void TestingSetup()
    {
    }
}