namespace scyna;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using NUnit.Framework;
using NATS.Client;
using NATS.Client.JetStream;

public abstract class EventTest
{
    private IMessage? event_;
    private IMessage? eventClone;
    private string channel = "";
    private string streamName = "";
    private bool exactEventMatch = true;

    public abstract void Run(IMessage message);

    public EventTest ExpectEvent(string channel, IMessage event_)
    {
        this.channel = channel;
        this.event_ = event_;
        return this;
    }

    public EventTest ExpectEvent(IMessage event_)
    {
        this.channel = "";
        this.event_ = event_;
        return this;
    }

    public EventTest MatchEvent<E>(string channel, E event_) where E : IMessage<E>, new()
    {
        this.channel = channel;
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public EventTest MatchEvent<E>(E event_) where E : IMessage<E>, new()
    {
        this.channel = "";
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    protected void createStream()
    {
        if (channel.Length == 0) return;
        streamName = Engine.Module;
        try
        {
            var config = StreamConfiguration.Builder()
                    .WithName(streamName)
                    .WithSubjects(streamName + ".>")
                    .Build();


            var jsm = Engine.Connection.CreateJetStreamManagementContext();
            if (jsm.GetStreamNames().Contains(streamName)) jsm.DeleteStream(streamName);
            jsm.AddStream(config);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Assert.True(false);
        }
    }

    protected void deleteStream()
    {
        if (channel.Length == 0) return;

        try
        {
            Engine.Connection.CreateJetStreamManagementContext().DeleteStream(streamName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Assert.True(false);
        }
    }

    protected void receiveEvent()
    {
        if (event_ == null || eventClone == null) return;

        if (channel.Length == 0)
        {
            var received = DomainEvent.NextEvent();
            if (received == null)
            {
                Assert.True(false);
                return;
            }
            if (exactEventMatch) Assert.True(event_.Equals(received));
            else Assert.True(partialMatchMessage(event_, received, eventClone));
        }
        else
        {
            try
            {
                var options = PushSubscribeOptions.Builder().WithStream(streamName).Build();
                var sub = Engine.Stream.PushSubscribeSync(streamName + "." + channel, options);
                var message = sub.NextMessage(1000); //1000ms
                if (message == null)
                {
                    Console.WriteLine("Timeout");
                    Assert.True(false);
                    return;
                }
                var ev = scyna.proto.Event.Parser.ParseFrom(message.Data);
                var parser = event_.Descriptor.Parser;
                var received = parser.ParseFrom(ev.Body);
                if (exactEventMatch) Assert.True(event_.Equals(received));
                else Assert.True(partialMatchMessage(event_, received, eventClone));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.True(false);
            }
        }
    }

    protected bool partialMatchMessage(IMessage x, IMessage y, IMessage xClone)
    {
        if (x.Descriptor != y.Descriptor) return false;

        var equal = true;

        var fields = y.Descriptor.Fields.InDeclarationOrder();

        foreach (var fd in fields)
        {
            fd.Accessor.Clear(xClone);
            var vx = fd.Accessor.GetValue(x);
            var vxClone = fd.Accessor.GetValue(xClone);

            if (!vx.Equals(vxClone))
            {
                var vy = fd.Accessor.GetValue(y);
                equal = vx.Equals(vy);//FIXME:
            }
            if (!equal) return false;
        }
        return equal;
    }

    public static DomainEventTest<T> ForHandler<T>(DomainEvent.Handler<T> handler) where T : IMessage<T>, new()
    {
        return new DomainEventTest<T>(handler);
    }

    public static IntegrationEventTest<T> ForHandler<T>(Event.Handler<T> handler) where T : IMessage<T>, new()
    {
        return new IntegrationEventTest<T>(handler);
    }

    public class DomainEventTest<T> : EventTest where T : IMessage<T>, new()
    {
        private DomainEvent.Handler<T> handler;
        public DomainEventTest(DomainEvent.Handler<T> handler) { this.handler = handler; }
        public override void Run(IMessage message)
        {
            createStream();
            var eventData = new DomainEvent.EventData(Engine.ID.Next(), message);
            handler.EventReceived(eventData);
            receiveEvent();
            deleteStream();
        }
    }

    public class IntegrationEventTest<T> : EventTest where T : IMessage<T>, new()
    {
        private Event.Handler<T> handler;
        public IntegrationEventTest(Event.Handler<T> handler) { this.handler = handler; }
        public override void Run(IMessage message)
        {
            createStream();
            var trace = Trace.NewEventTrace("");
            handler.Init(trace);
            handler.MessageReceived(message.ToByteArray());
            receiveEvent();
            deleteStream();
        }
    }
}
