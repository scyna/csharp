namespace ex.Basic;
using scyna;

public class TestSignal : Signal.StatefulHandler<proto.TestSignal>
{
    public override void Execute()
    {
        Console.WriteLine("Receive TestSignal:" + data.Text);
    }
}

class EmptySignal : Signal.StatelessHandler
{
    public override void Execute()
    {
        Console.WriteLine("Receive EmptySignal");
    }
}
