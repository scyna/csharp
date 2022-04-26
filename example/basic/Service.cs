namespace ex.Basic;
using scyna;

public class HelloService : Service.StatelessHandler
{
    public override void Execute()
    {
        Console.WriteLine("Receive HelloRequest");
        LOG.Info("Test Service log from HelloService");
        Done(new Proto.HelloResponse { Text = "Hello World" });
    }
}

public class EchoService : Service.StatefulHandler<Proto.EchoRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive EchoRequest");
        Done(new Proto.EchoResponse { Text = request.Text });
    }
}

public class AddService : Service.StatefulHandler<Proto.AddRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive EchoRequest");
        var sum = request.A + request.B;
        if (sum > 100) Error(ex.Basic.Error.TOO_BIG);
        else Done(new Proto.AddResponse { Sum = sum });
    }
}
