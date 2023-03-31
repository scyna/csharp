namespace ex.registering;

using scyna;

class Writer
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "ex_account", "123456");
        Endpoint.Register(Path.REGISTER_USER_URL, new RegisterUser());
        Engine.Start();
    }
}