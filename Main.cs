using scyna;

namespace Test
{
    class EmptyService : Service.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive EchoRequest");
            log.Info("Test Log from echo [C#]");
            Error(scyna.Error.REQUEST_INVALID);
            Done(new scyna.proto.Response { });
        }
    }
    class EchoService : Service.Handler<scyna.proto.Request>
    {
        public override void Execute(scyna.proto.Request request)
        {
            Console.WriteLine("Receive EchoRequest");
            log.Info("Test Log from echo [C#]");
            Error(scyna.Error.REQUEST_INVALID);
            Done(new scyna.proto.Response { });
        }
    }
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

            Service.Register("", new EchoService());
            Service.Register("", new EmptyService());

            Engine.LOG.Error("Test log form c#");
            Console.WriteLine("Test ID Generator:" + Engine.ID.next());
            Console.WriteLine("Engine Stopped");
        }
    }
}