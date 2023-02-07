namespace ex.hello.Test;

using NUnit.Framework;
using scyna;
using ex.hello;

[TestFixture]
class EchoTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.ECHO_URL, new EchoService());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Engine.Release();
    }

    [Test]
    public void TestEchoSuccess()
    {
        scyna.EndpointTest.Create(Path.ECHO_URL)
            .WithRequest(new proto.EchoRequest { Text = "Hello" })
            .ExpectResponse(new proto.EchoResponse { Text = "Hello" })
            .Run();
    }
}