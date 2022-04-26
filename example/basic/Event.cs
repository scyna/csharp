namespace ex.Basic;
using scyna;

public class TestEvent : Event.Handler<Proto.TestEvent>
{
    public override void Execute()
    {
        Console.WriteLine("Receive TestEvent:" + data.Text);
    }
}
