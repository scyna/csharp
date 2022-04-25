using scyna;

namespace Example;

class EchoTest
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

        Service.Register("/example/echo", new EchoService());
        Service.Register("/example/hello", new HelloService());
        Signal.Register("example.signal.test", new TestSignal());
        Signal.Register("example.signal.empty", new EmptySignal());
        Event.Register("example.event.test", "consumer", new TestEvent());

        Engine.Start();
        Console.WriteLine("Engine Stopped");
    }
}
