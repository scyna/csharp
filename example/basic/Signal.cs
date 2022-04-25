using scyna;
namespace Example;

public class TestSignal : Signal.StatefulHandler<example.TestSignal>
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
