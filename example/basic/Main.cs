namespace ex.Basic;
using scyna;

class Basic
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

        Service.Register("/example/basic/echo", new EchoService());
        Service.Register("/example/basic/hello", new HelloService());
        Service.Register("/example/basic/add", new HelloService());

        Signal.Register("example.basic.signal.test", new TestSignal());
        Signal.Register("example.basic.signal.empty", new EmptySignal());
        //Event.Register("example.basic.event.test", "consumer", new TestEvent());

        Engine.Start();
    }
}
