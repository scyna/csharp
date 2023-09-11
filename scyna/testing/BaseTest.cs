namespace scyna;
using Google.Protobuf;
using Xunit;
using NATS.Client.JetStream;

public class BaseTest<M> where M : BaseTest<M>
{
    private IMessage? event_;
    private string channel = "";
    private string streamName = "";
    private readonly List<DomainEventItem> domainEvents = new();

    public M ExpectEvent(string channel, IMessage event_)
    {
        this.channel = channel;
        this.event_ = event_;
        return (M)this;
    }

    public M ExpectDomainEvent(IMessage event_)
    {
        domainEvents.Add(DomainEventItem.Create(event_));
        return (M)this;
    }

    public M ExpectDomainEvent<T>() where T : IMessage<T>, new()
    {
        domainEvents.Add(DomainEventItem.CreateEmpty<T>());
        return (M)this;
    }

    public M DomainEventRaised<T>(out T event_) where T : IMessage<T>, new()
    {
        var received = DomainEvent.NextEvent();

        if (received is not null)
        {
            var data = received.ToByteString();
            MessageParser<T> parser = new(() => new T());
            event_ = parser.ParseFrom(data);
        }
        else event_ = new T();
        return (M)this;
    }

    public M EventPublished<T>(string channel, out T event_) where T : IMessage<T>, new()
    {
        if (this.event_ is not null) throw new Exception("Event is received with ExpectEvent()");

        this.channel = channel;

        var options = PushSubscribeOptions.Builder().WithStream(streamName).Build();
        var sub = Engine.Stream.PushSubscribeSync(streamName + "." + channel, options);
        var message = sub.NextMessage(1000); //1000ms
        if (message is not null)
        {
            var ev = scyna.proto.Event.Parser.ParseFrom(message.Data);
            MessageParser<T> parser = new(() => new T());
            event_ = parser.ParseFrom(ev.Body);
        }
        else throw new Exception("No event received");

        return (M)this;
    }

    protected void CreateStream()
    {
        streamName = Engine.Module;
        var config = StreamConfiguration.Builder()
            .WithName(streamName)
            .WithSubjects($"{streamName}.>")
            .Build();

        var jsm = Engine.Connection.CreateJetStreamManagementContext();
        if (jsm.GetStreamNames().Contains(streamName)) jsm.DeleteStream(streamName);
        jsm.AddStream(config);
    }

    protected void DeleteStream()
    {
        Engine.Connection.CreateJetStreamManagementContext().DeleteStream(streamName);
    }

    protected void ReceiveDomainEvent()
    {
        if (!domainEvents.Any()) return;
        foreach (var domainEvent in domainEvents)
        {
            var received = DomainEvent.NextEvent() ?? throw new Exception("No event received");
            if (domainEvent.Empty) Assert.Equal(domainEvent.Event.GetType(), received.GetType());
            else Assert.True(domainEvent.Equals(received), "DomainEvent is not equal");
        }
    }

    protected void ReceiveEvent()
    {
        if (event_ is null) return;

        var options = PushSubscribeOptions.Builder().WithStream(streamName).Build();
        var sub = Engine.Stream.PushSubscribeSync($"{streamName}.{channel}", options);
        var message = sub.NextMessage(1000) ?? throw new Exception("No event received"); //1000ms
        var ev = scyna.proto.Event.Parser.ParseFrom(message.Data);
        var parser = event_.Descriptor.Parser;
        var received = parser.ParseFrom(ev.Body);
        Assert.True(event_.Equals(received));
    }

    class DomainEventItem
    {
        public IMessage Event { get; }
        public bool Empty { get; } = false;
        private DomainEventItem(IMessage event_, bool empty) { Event = event_; Empty = empty; }
        public static DomainEventItem CreateEmpty<T>() where T : IMessage<T>, new() => new(new T(), true);
        public static DomainEventItem Create(IMessage event_) => new(event_, false);
    }
}