namespace ex.account;

using scyna;

class Writer
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "ex_account", "123456");
        Engine.Start();
    }
}