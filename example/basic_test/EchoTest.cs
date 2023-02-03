namespace ex.Basic.Test;
using NUnit.Framework;
using scyna;
using ex.Basic;

[TestFixture]
class EchoTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Endpoint.Register(Path.ECHO_USER_URL, new EchoService());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Engine.Release();
    }

    [Test]
    public void TestEchoSuccess()
    {
        scyna.EndpointTest.New(Path.ECHO_USER_URL)
            .WithRequest(new proto.EchoRequest { Text = "Hello" })
            .ExpectResponse(new proto.EchoResponse { Text = "Hello" })
            .Run();
    }

    [Test]
    public void TestEchoCode()
    {
        scyna.EndpointTest.New(Path.ECHO_USER_URL)
            .WithRequest(new proto.EchoRequest { Text = "Hello" })
            .ExpectSuccess()
            .Run();
    }

    [Test]
    public void TestCallService()
    {
        var r = scyna.EndpointTest.New(Path.ECHO_USER_URL)
            .WithRequest(new proto.EchoRequest { Text = "Hello" })
            .Run<proto.EchoResponse>();

        Assert.AreEqual("Hello", r.Text);
    }
}