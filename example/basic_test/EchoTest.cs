namespace ex.Basic.Test;
using NUnit.Framework;
using scyna;
using ex.Basic;

[TestFixture]
class EchoTest
{
    [SetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/basic/echo", new EchoService());
    }

    [Test]
    public void TestEchoSuccess()
    {
        scyna.Test.TestService(
            "/example/basic/echo",
            new Proto.EchoRequest { Text = "Hello" },
            new Proto.EchoResponse { Text = "Hello" },
            200
        );
    }

    [Test]
    public void TestEchoCode()
    {
        scyna.Test.TestService("/example/basic/echo", new Proto.EchoRequest { Text = "Hello" }, 200);
    }

    [Test]
    public void TestCallService()
    {
        var r = scyna.Test.CallService<Proto.EchoResponse>("/example/basic/echo", new Proto.EchoRequest { Text = "echo" });
        Assert.AreEqual(r.Text, "echo");
    }
}