namespace Registering;

using scyna;

class Registering
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "ex_account", "123456");
        Endpoint.Register(Path.REGISTER_USER_URL, new RegisterUserHandler());
        Engine.Start();
    }
}