using scyna;

namespace Test
{
    class HelloService : Service.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive HelloRequest");
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

            Service.Register("", new EchoService());
            Service.Register("", new HelloService());

            Engine.LOG.Error("Test log form c#");
            Console.WriteLine("Test ID Generator:" + Engine.ID.next());
            Console.WriteLine("Engine Stopped");
        }
    }
}