namespace ex.hello;

using scyna;

class Hello
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Engine.Start();
    }
}