using scyna;
namespace Example;

public class HelloService : Service.StatelessHandler
{
    public override void Execute()
    {
        Console.WriteLine("Receive HelloRequest");
        LOG.Info("Test Service log from HelloService");
        Done(new example.HelloResponse { Text = "Hello World" });
    }
}

public class EchoService : Service.StatefulHandler<example.EchoRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive EchoRequest");
        Done(new example.EchoResponse { Text = request.Text });
    }
}
