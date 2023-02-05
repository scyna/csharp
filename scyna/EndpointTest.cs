namespace scyna;
using Google.Protobuf;
using NUnit.Framework;

public class EndpointTest
{
    private string url;
    private IMessage? request;
    private IMessage? response;
    private int status;

    private EndpointTest(string url) { this.url = url; }

    public static EndpointTest New(string url)
    {
        return new EndpointTest(url);
    }

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
}