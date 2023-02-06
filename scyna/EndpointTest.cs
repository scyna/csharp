namespace scyna;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using NUnit.Framework;

public class EndpointTest
{
    private int status;
    private string url;
    private IMessage? request;
    private IMessage? response;
    private IMessage? event_;
    private string channel = "";
    private string streamName = "";
    private bool exactEventMatch = true;
    private bool exactResponseMatch = true;

    private EndpointTest(string url) { this.url = url; }
    public static EndpointTest New(string url) { return new EndpointTest(url); }

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

    public EndpointTest ExpectEvent(IMessage event_)
    {
        this.event_ = event_;
        return this;
    }

    public EndpointTest MatchEvent(IMessage event_)
    {
        this.exactEventMatch = false;
        this.event_ = event_;
        return this;
    }

    public EndpointTest PublishEventTo(String channel)
    {
        this.channel = channel;
        return this;
    }

    public EndpointTest ExpectError(proto.Error error)
    {
        this.status = 400;
        this.response = error;
        return this;
    }

    public EndpointTest ExpectResponse(IMessage response)
    {
        this.status = 200;
        this.response = response;
        return this;
    }

    public EndpointTest MatchResponse(IMessage response)
    {
        this.exactResponseMatch = false;
        this.status = 200;
        this.response = response;
        return this;
    }


    public void Run()
    {
        var res = Request.Send(url, request);
        Assert.IsNotNull(res);
        Assert.AreEqual(status, res.Code);
        if (response != null)
        {
            var parser = response.Descriptor.Parser;
            var r = parser.ParseFrom(res.Body);
            Assert.IsTrue(response.Equals(r));
        }
    }

    public T Run<T>() where T : IMessage<T>, new()
    {
        var res = Request.Send(url, request);
        Assert.IsNotNull(res);
        Assert.AreEqual(200, res.Code);
        MessageParser<T> parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(res.Body);
    }

    private void createStream()
    {
        if (channel.Length == 0) return;
        streamName = Engine.Module;
        // try
        // {
        //     StreamConfiguration config = StreamConfiguration.builder()
        //             .name(streamName)
        //             .subjects(streamName + ".>")
        //             .build();

        //     var jsm = Engine.connection().jetStreamManagement();
        //     if (jsm.getStreamNames().contains(streamName))
        //     {
        //         jsm.deleteStream(streamName);
        //     }
        //     jsm.addStream(config);
        // }
        // catch (Exception e)
        // {
        //     e.printStackTrace();
        //     assertTrue("Error in creating stream", false);
        // }
    }

    private bool partialMatchMessage(IMessage x, IMessage y)
    {
        if (x.Descriptor != y.Descriptor) return false;

        var equal = true;

        var fields = y.Descriptor.Fields.InFieldNumberOrder();

        foreach (var fd in fields)
        {
            if (fd.Accessor.HasValue(x))
            {
                var vx = fd.Accessor.GetValue(x);
                var vy = fd.Accessor.GetValue(y);
                equal = vx.Equals(vy);//FIXME:
            }
            if (!equal) return false;
        }
        return equal;
    }
}