using NUnit.Framework;
using scyna;
using Example;

namespace test;

[TestFixture]
class EchoTest
{
    [SetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/echo", new EchoService());
    }

    [Test]
    public void TestEchoSuccess()
    {
        scyna.Test.TestService(
            "/example/echo",
            new example.EchoRequest { Text = "Hello" },
            new example.EchoResponse { Text = "Hello" },
            200
        );
    }

    [Test]
    public void TestEchoCode()
    {
        scyna.Test.TestService("/example/echo", new example.EchoRequest { Text = "Hello" }, 200);
    }

    [Test]
    public void TestCallService()
    {
        var r = scyna.Test.CallService<example.EchoResponse>("/example/echo", new example.EchoRequest { Text = "echo" });
        Assert.AreEqual(r.Text, "echo");
    }
}