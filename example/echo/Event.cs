using scyna;
namespace Example
{
    class TestEvent : Event.Handler<example.TestEvent>
    {
        public override void Execute(example.TestEvent data)
        {
            Console.WriteLine("Receive TestEvent:" + data.Text);
        }
    }

}