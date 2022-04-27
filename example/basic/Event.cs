namespace ex.Basic;
using scyna;

public class TestEvent : Event.Handler<proto.TestEvent>
{
    public override void Execute()
    {
        Console.WriteLine("Receive TestEvent:" + data.Text);
    }
}
