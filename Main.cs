using scyna;

namespace Test
{
    class HelloService : Service.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive HelloRequest");
            LOG.Info("Test Service log from HelloService");
            Done(new example.HelloResponse { Text = "Hello World" });
        }
    }
    class EchoService : Service.Handler<example.EchoRequest>
    {
        public override void Execute(example.EchoRequest request)
        {
            Console.WriteLine("Receive EchoRequest");
            Done(new example.EchoResponse { Text = request.Text });
        }
    }

    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

            Service.Register("/example/echo", new EchoService());
            Service.Register("/example/hello", new HelloService());

            Engine.LOG.Error("Test log form c#");
            Console.WriteLine("Test ID Generator:" + Engine.ID.next());

            var response = Service.SendRequest<example.EchoResponse>("/example/echo", new example.EchoRequest { Text = "Echo" });
            if (response != null) Console.WriteLine("Echo Response:" + response.Text);

            var hello = Service.SendRequest<example.HelloResponse>("/example/hello", null);
            if (hello != null) Console.WriteLine("Hello Response:" + hello.Text);

            Console.WriteLine("Engine Stopped");
        }
    }
}