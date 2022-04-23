using scyna;

namespace Test
{

    class EchoService : Service.Base<scyna.proto.Request>
    {
        public override void Execute()
        {
            Console.WriteLine("Receive EchoRequest");
            var request = parse();
            if (request == null) return;
            log.Info("Test Log from echo [C#]");
            //Done(new  EchoResponse.newBuilder().setText(request.getText()).build());
        }
    }
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
            Engine.LOG.Error("Test log form c#");
            Console.WriteLine("Test ID Generator:" + Engine.ID.next());
            Console.WriteLine("Engine Stopped");
        }
    }
}