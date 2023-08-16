namespace scyna;
using Google.Protobuf;
using Xunit;
using NATS.Client.JetStream;

public abstract class EventTest
{
    private IMessage? event_;
    private IMessage? eventClone;
    private IMessage? input;
    private string channel = "";
    private string streamName = "";
    private bool exactEventMatch = true;
    private ByteString? eventData;
    private bool expectSuccess = true;

    public abstract EventTest Run();

    public EventTest WithData(IMessage data)
    {
        this.input = data;
        return this;
    }

    public EventTest ExpectEvent(IMessage event_)
    {
        this.eventClone = event_;
        this.event_ = event_;
        return this;
    }

    public EventTest ExpectSucess()
    {
        expectSuccess = true;
        return this;
    }

    public EventTest ExpectError()
    {
        expectSuccess = false;
        return this;
    }



    public EventTest ExpectEventLike<E>(E event_) where E : IMessage<E>, new()
    {
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public void DecodeEvent<T>(out T event_) where T : IMessage<T>, new()
    {
        if (eventData is null)
        {
            if (channel.Length == 0)
            {
                var received = DomainEvent.NextEvent();
                if (received != null) eventData = received.ToByteString();
            }
            else
            {
                try
                {
                    var options = PushSubscribeOptions.Builder().WithStream(streamName).Build();
                    var sub = Engine.Stream.PushSubscribeSync($"{streamName}.{channel}", options);
                    var message = sub.NextMessage(1000); //1000ms
                    if (message != null)
                    {
                        var ev = scyna.proto.Event.Parser.ParseFrom(message.Data);
                        var tmp = new T();
                        var parser = tmp.Descriptor.Parser;
                        var received = parser.ParseFrom(ev.Body);
                        eventData = ev.Body;
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        if (eventData is null) event_ = new T();
        else
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            event_ = parser.ParseFrom(eventData);
        }
    }

    public EventTest FromChannel(string channel)
    {
        this.channel = channel;
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
                    .WithSubjects($"{streamName}.>")
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
            eventData = received.ToByteString();
            if (exactEventMatch) Assert.True(event_.Equals(received));
            else Assert.True(partialMatchMessage(event_, received, eventClone));
        }
        else
        {
            try
            {
                var options = PushSubscribeOptions.Builder().WithStream(streamName).Build();
                var sub = Engine.Stream.PushSubscribeSync($"{streamName}.{channel}", options);
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
                eventData = ev.Body;
                if (exactEventMatch) Assert.True(event_.Equals(received));
                else
                {
                    if (eventClone == null) Assert.True(false);
                    else Assert.True(partialMatchMessage(event_, received, eventClone));
                }
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

    public static DomainEventTest<T> Create<T>(DomainEvent.Handler<T> handler) where T : IMessage<T>, new()
    {
        return new DomainEventTest<T>(handler);
    }

    public static IntegrationEventTest<T> Create<T>(Event.Handler<T> handler) where T : IMessage<T>, new()
    {
        return new IntegrationEventTest<T>(handler);
    }

    public static TaskTest<T> Create<T>(Task.Handler<T> handler) where T : IMessage<T>, new()
    {
        return new TaskTest<T>(handler);
    }

    public class DomainEventTest<T> : EventTest where T : IMessage<T>, new()
    {
        private DomainEvent.Handler<T> handler;
        public DomainEventTest(DomainEvent.Handler<T> handler) { this.handler = handler; }
        public override EventTest Run()
        {
            DomainEvent.Clear();
            try
            {
                if (this.input == null) Assert.True(false);

                createStream();
                var eventData = new DomainEvent.EventData((ulong)Engine.ID.Next(), input);
                handler.TestEventReceived(eventData);
                receiveEvent();
                deleteStream();
                if (expectSuccess) return this;
                expectSuccess = true;
                Assert.False(expectSuccess);
            }
            catch
            {
                Console.WriteLine($"Expect Success {expectSuccess}");
                Assert.False(expectSuccess);
            }
            return this;
        }
    }

    public class IntegrationEventTest<T> : EventTest where T : IMessage<T>, new()
    {
        private Event.Handler<T> handler;
        public IntegrationEventTest(Event.Handler<T> handler) { this.handler = handler; }
        public override EventTest Run()
        {
            if (this.input == null)
            {
                Assert.True(false);
                return this;
            }

            DomainEvent.Clear();

            createStream();
            var trace = Trace.NewEventTrace("");
            handler.Init(trace);

            scyna.proto.Event event_ = new proto.Event
            {
                Body = input.ToByteString(),
                TraceID = 0,
            };

            handler.MessageReceived(event_.ToByteArray());
            receiveEvent();
            deleteStream();
            return this;
        }
    }

    public class TaskTest<T> : EventTest where T : IMessage<T>, new()
    {
        private Task.Handler<T> handler;
        public TaskTest(Task.Handler<T> handler) { this.handler = handler; }
        public override EventTest Run()
        {
            if (this.input == null)
            {
                Assert.True(false);
                return this;
            }

            createStream();

            var task = new scyna.proto.Task
            {
                TraceID = (ulong)Engine.ID.Next(),
                Data = input.ToByteString(),
            };

            handler.MessageReceived(task.ToByteArray());
            receiveEvent();
            deleteStream();
            return this;
        }
    }
}
