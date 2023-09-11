namespace scyna;
using Google.Protobuf;
using Xunit;

public class DomainEventTest<T> : BaseTest<DomainEventTest<T>>
    where T : IMessage<T>, new()
{
    private readonly DomainEvent.Handler<T> handler;
    private IMessage? input;
    private bool expectSuccess = true;

    internal DomainEventTest(DomainEvent.Handler<T> handler) { this.handler = handler; }

    public DomainEventTest<T> WithData(IMessage data)
    {
        this.input = data;
        return this;
    }

    public DomainEventTest<T> ShouldBeFine()
    {
        expectSuccess = true;
        return Run();
    }

    public DomainEventTest<T> ShouldFail()
    {
        expectSuccess = false;
        return Run();
    }

    private DomainEventTest<T> Run()
    {
        DomainEvent.Clear();
        try
        {
            if (input is null) throw new Exception("Input is null");

            CreateStream();
            var eventData = new DomainEvent.EventData((ulong)Engine.ID.Next(), input);
            handler.TestEventReceived(eventData);
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
