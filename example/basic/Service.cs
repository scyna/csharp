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

public class AddService : Service.StatefulHandler<example.AddRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive EchoRequest");
        var sum = request.A + request.B;
        if (sum > 100) Error(Example.Error.TOO_BIG);
        else Done(new example.AddResponse { Sum = sum });
    }
}
