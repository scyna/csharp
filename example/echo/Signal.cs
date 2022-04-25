using scyna;
namespace Example
{
    class TestSignal : Signal.StatefulHandler<example.TestSignal>
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
}