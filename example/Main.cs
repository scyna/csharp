using scyna;

namespace Example
{
    class EchoTest
    {
        //     static void Main(string[] args)
        //     {
        //         Console.WriteLine("Hello World!");
        //         Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

        //         Service.Register("/example/echo", new EchoService());
        //         Service.Register("/example/hello", new HelloService());
        //         Signal.Register("example.signal.test", new TestSignal());
        //         Signal.Register("example.signal.empty", new EmptySignal());
        //         //Event.Register("example.event.test", "consumer", new TestEvent());

        //         Engine.LOG.Error("Test log form c#");
        //         Console.WriteLine("Test ID Generator:" + Engine.ID.Next());

        //         var response = Service.SendRequest<example.EchoResponse>("/example/echo", new example.EchoRequest { Text = "Echo" });
        //         if (response != null) Console.WriteLine("Echo Response:" + response.Text);

        //         var hello = Service.SendRequest<example.HelloResponse>("/example/hello", null);
        //         if (hello != null) Console.WriteLine("Hello Response:" + hello.Text);

        //         Signal.Emit("example.signal.test", new example.TestSignal { Text = "test signal" });
        //         Signal.Emit("example.signal.empty");

        //         //Event.Push("example.event.test", new example.TestEvent { Text = "test event" });

        //         Engine.Start();
        //         Console.WriteLine("Engine Stopped");
        //     }
    }
}