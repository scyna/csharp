using scyna;
namespace Example
{
    class TestEvent : Event.Handler<example.TestEvent>
    {
        public override void Execute()
        {
            Console.WriteLine("Receive TestEvent:" + data.Text);
        }
    }

}