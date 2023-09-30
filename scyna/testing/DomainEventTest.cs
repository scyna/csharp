namespace scyna;
using Google.Protobuf;
using Xunit;

public class DomainEventTest<T> : BaseTest<DomainEventTest<T>>
    where T : IMessage<T>, new()
{
    private readonly DomainEvent.Handler<T> handler;
    private IMessage? input;
    private scyna.Error? error;

    internal DomainEventTest(DomainEvent.Handler<T> handler) { this.handler = handler; }

    public DomainEventTest<T> WithData(IMessage data)
    {
        this.input = data;
        return this;
    }

    public DomainEventTest<T> ExpectError(Error error)
    {
        this.error = error;
        return this;
    }

    public DomainEventTest<T> Run()
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
        }
        catch (Error e)
        {
            if (error is null) throw;
            Assert.Equal(error.Code, e.Code);
            Assert.Equal(error.Message, e.Message);
        }
        catch { throw; }
        return this;
    }
}
