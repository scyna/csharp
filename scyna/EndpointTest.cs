namespace scyna;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Xunit;
using NATS.Client;
using NATS.Client.JetStream;

public class EndpointTest
{
    private int status;
    private string url;
    private IMessage? request;
    private IMessage? response;
    private IMessage? responseClone;
    private IMessage? event_;
    private IMessage? eventClone;
    private string channel = "";
    private string streamName = "";
    private bool exactEventMatch = true;
    private bool exactResponseMatch = true;

    private EndpointTest(string url) { this.url = url; }
    public static EndpointTest Create(string url) { return new EndpointTest(url); }

    public EndpointTest WithRequest(IMessage request)
    {
        this.request = request;
        return this;
    }

    public EndpointTest ExpectSuccess()
    {
        this.status = 200;
        return this;
    }

    public EndpointTest ExpectEvent(string channel, IMessage event_)
    {
        this.channel = channel;
        this.eventClone = event_;
        this.event_ = event_;
        return this;
    }

    public EndpointTest ExpectEvent(IMessage event_)
    {
        this.channel = "";
        this.eventClone = event_;
        this.event_ = event_;
        return this;
    }

    public EndpointTest MatchEvent<T>(string channel, T event_) where T : IMessage<T>, new()
    {
        this.channel = channel;
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public EndpointTest MatchEvent<T>(T event_) where T : IMessage<T>, new()
    {
        this.channel = "";
        this.eventClone = event_.Clone();
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public EndpointTest ExpectError(scyna.Error error)
    {
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

    public EndpointTest MatchResponse<T>(T response) where T : IMessage<T>, new()
    {
        this.responseClone = response.Clone();
        this.exactResponseMatch = false;
        this.status = 200;
        this.response = response;
        return this;
    }

    public void Run()
    {
        createStream();
        var res = Request.Send(url, request);
        Assert.NotNull(res);
        Assert.Equal(status, res.Code);
        if (response != null && responseClone != null)
        {
            var parser = response.Descriptor.Parser;
            var r = parser.ParseFrom(res.Body);
            Assert.True(response.Equals(r));
            if (exactResponseMatch) Assert.True(response.Equals(r));
            else Assert.True(partialMatchMessage(response, r, responseClone));
        }
        receiveEvent();
        deleteStream();
    }

    public T Run<T>() where T : IMessage<T>, new()
    {
        var res = Request.Send(url, request);
        Assert.NotNull(res);
        Assert.Equal(200, res.Code);
        MessageParser<T> parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(res.Body);
    }

    private void createStream()
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
        if (event_ == null) return;

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