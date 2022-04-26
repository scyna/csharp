namespace ex.Basic;
using scyna;

public class TestSignal : Signal.StatefulHandler<Proto.TestSignal>
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
