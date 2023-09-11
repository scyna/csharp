namespace scyna;
using Google.Protobuf;
using Xunit;

public class EventTest<T> : BaseTest<EventTest<T>>
    where T : IMessage<T>, new()
{
    private readonly Event.Handler<T> handler;
    private IMessage? input;
    private bool expectSuccess = true;

    internal EventTest(Event.Handler<T> handler) { this.handler = handler; }

    public EventTest<T> WithData(IMessage data)
    {
        this.input = data;
        return this;
    }

    public EventTest<T> ShouldBeFine()
    {
        expectSuccess = true;
        return Run();
    }

    public EventTest<T> ShouldFail()
    {
        expectSuccess = false;
        return Run();
    }

    private EventTest<T> Run()
    {
        DomainEvent.Clear();
        try
        {
            if (input is null) throw new Exception("Input is null");
            CreateStream();
            var trace = Trace.NewEventTrace("");
            handler.Init(trace);

            proto.Event event_ = new()
            {
                Body = input.ToByteString(),
                TraceID = 0,
            };

            handler.MessageReceived(event_.ToByteArray());
            ReceiveDomainEvent();
            ReceiveEvent();
            DeleteStream();
            if (expectSuccess) return this;
            expectSuccess = true;
            Assert.False(expectSuccess);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Assert.False(expectSuccess);
        }
        return this;
    }
}
