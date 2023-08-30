namespace scyna;
using Google.Protobuf;
using Xunit;
using NATS.Client.JetStream;

public class EndpointTest
{
    private int status;
    private readonly string url;
    private IMessage? request;
    private IMessage? metadata;
    private IMessage? response;
    private IMessage? responseClone;
    private IMessage? event_;
    private IMessage? eventClone;
    private string channel = "";
    private string streamName = "";
    private bool exactEventMatch = true;
    private bool exactResponseMatch = true;

    private ByteString? responseData;
    private ByteString? eventData;

    private EndpointTest(string url) { this.url = url; }
    public static EndpointTest Create(string url) { return new EndpointTest(url); }

    public EndpointTest WithRequest(IMessage request)
    {
        this.request = request;
        return this;
    }

    public EndpointTest WithMetadata(IMessage data)
    {
        this.metadata = data;
        return this;
    }

    public EndpointTest ExpectSuccess()
    {
        this.status = 200;
        return this;
    }

    public EndpointTest FromChannel(string channel)
    {
        this.channel = channel;
        return this;
    }

    public EndpointTest ExpectEvent(IMessage event_)
    {
        this.eventClone = event_;
        this.event_ = event_;
        return this;
    }

    public EndpointTest ExpectEventLike<T>(T event_) where T : IMessage<T>, new()
    {
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public EndpointTest ExpectError(scyna.Error error)
    {
        this.exactResponseMatch = true;
        this.status = 400;
        this.response = error.ToProto();
        return this;
    }

    public EndpointTest ExpectResponse(IMessage response)
    {
        this.status = 200;
        this.responseClone = response;
        this.response = response;
        return this;
    }

    public EndpointTest ExpectResponseLike<T>(T response) where T : IMessage<T>, new()
    {
        this.responseClone = response.Clone();
        this.exactResponseMatch = false;
        this.status = 200;
        this.response = response;
        return this;
    }

    public EndpointTest DecodeResponse<T>(out T response) where T : IMessage<T>, new()
    {
        MessageParser<T> parser = new(() => new T());
        response = parser.ParseFrom(responseData);
        return this;
    }


    public EndpointTest DecodeEvent<T>(out T event_) where T : IMessage<T>, new()
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
                    var sub = Engine.Stream.PushSubscribeSync(streamName + "." + channel, options);
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
            MessageParser<T> parser = new(() => new T());
            event_ = parser.ParseFrom(eventData);
        }

        return this;
    }

    public EndpointTest Run()
    {
        DomainEvent.Clear();
        createStream();
        var res = Request.Send(url, request, metadata);
        if (res is null)
        {
            throw new Exception("Response is NULL");
        }
        Assert.Equal(status, res.Code);
        if (this.response != null)
        {
            var parser = response.Descriptor.Parser;
            var r = parser.ParseFrom(res.Body);
            if (exactResponseMatch) Assert.True(response.Equals(r));
            else if (responseClone is not null) Assert.True(partialMatchMessage(response, r, responseClone));

        }
        receiveEvent();
        deleteStream();
        responseData = res.Body;
        return this;
    }

    private void createStream()
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

    private void deleteStream()
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

    private void receiveEvent()
    {
        if (event_ is null || eventClone is null) return;

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

    private bool partialMatchMessage(IMessage x, IMessage y, IMessage xClone)
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
}