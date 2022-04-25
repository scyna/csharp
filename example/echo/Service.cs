using scyna;
namespace Example
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
        public override void Execute()
        {
            Console.WriteLine("Receive EchoRequest");
            Done(new example.EchoResponse { Text = request.Text });
        }
    }

}