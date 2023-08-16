namespace ex.Registering;

using scyna;

class Registering
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "ex_account", "123456");
        Endpoint.Register(Path.CREATE_REGISTRATION, new CreateRegistrationHandler());
        Engine.Start();
    }
}