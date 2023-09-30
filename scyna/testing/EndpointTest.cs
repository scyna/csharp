namespace scyna;
using Google.Protobuf;
using Xunit;

public class EndpointTest : BaseTest<EndpointTest>
{
    private int status = 200;
    private readonly string url;
    private IMessage? request;
    private IMessage? metadata;
    private IMessage? response;
    private proto.Error? error;
    private ByteString? responseData;
    private bool emptyResponse = false;

    internal EndpointTest(string url) { this.url = url; }

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

    public EndpointTest ExpectError(scyna.Error error)
    {
        this.status = 400;
        this.error = error.ToProto();
        return this;
    }

    public EndpointTest ExpectResponse(IMessage response)
    {
        this.status = 200;
        emptyResponse = false;
        this.response = response;
        return this;
    }

    public EndpointTest ExpectResponse<T>() where T : IMessage<T>, new()
    {
        this.status = 200;
        emptyResponse = true;
        this.response = new T();
        return this;
    }

    public EndpointTest GotResponse<T>(out T response) where T : IMessage<T>, new()
    {
        MessageParser<T> parser = new(() => new T());
        response = parser.ParseFrom(responseData);
        return this;
    }

    public EndpointTest Run()
    {
        DomainEventQueue.Clear();
        CreateStream();
        var res = Request.Send(url, request, metadata) ?? throw new Exception("Response is null, please check if the endpoint is registered");
        Assert.Equal(status, res.Code);

        if (error is not null)
        {
            var r = proto.Error.Parser.ParseFrom(res?.Body);
            Assert.Equal(error.Code, r.Code);
            Assert.Equal(error.Message, r.Message);
        }
        else if (response is not null)
        {
            var r = response.Descriptor.Parser.ParseFrom(res?.Body);
            if (emptyResponse) Assert.True(r.GetType() == response.GetType(), "Response type is not equal");
            else Assert.True(response.Equals(r), "Response is not equal");
        }

        ReceiveDomainEvent();
        ReceiveEvent();
        DeleteStream();
        responseData = res?.Body;
        return this;
    }
}