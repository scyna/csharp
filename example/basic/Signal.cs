namespace ex.Basic;
using scyna;

public class TestSignal : Signal.Handler<proto.TestSignal>
{
    public override void Execute()
    {
        Console.WriteLine("Receive TestSignal:" + data.Text);
    }
}

