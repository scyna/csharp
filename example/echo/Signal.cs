using scyna;
namespace Example
{
    class TestSignal : Signal.Handler<example.TestSignal>
    {
        public override void Execute()
        {
            Console.WriteLine("Receive TestSignal:" + data.Text);
        }
    }

    class EmptySignal : Signal.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive EmptySignal");
        }
    }
}