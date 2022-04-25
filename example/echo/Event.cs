using scyna;
namespace Example;

public class TestEvent : Event.Handler<example.TestEvent>
{
    public override void Execute()
    {
        Console.WriteLine("Receive TestEvent:" + data.Text);
    }
}
