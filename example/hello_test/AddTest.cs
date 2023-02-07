namespace ex.hello.Test;

using NUnit.Framework;
using scyna;
using ex.hello;

[TestFixture]
class AddTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.ADD_URL, new AddService());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Engine.Release();
    }

    [Test]
    public void TestAddSuccess()
    {
        scyna.EndpointTest.Create(Path.ADD_URL)
            .WithRequest(new proto.AddRequest { A = 3, B = 5 })
            .ExpectResponse(new proto.AddResponse { Sum = 8 })
            .Run();

        scyna.EndpointTest.Create(Path.ADD_URL)
            .WithRequest(new proto.AddRequest { A = 9, B = 91 })
            .ExpectResponse(new proto.AddResponse { Sum = 100 })
            .Run();
    }

    [Test]
    public void TestAddTooBig()
    {
        scyna.EndpointTest.Create(Path.ADD_URL)
            .WithRequest(new proto.AddRequest { A = 90, B = 75 })
            .ExpectError(Error.REQUEST_INVALID)
            .Run();
        scyna.EndpointTest.Create(Path.ADD_URL)
            .WithRequest(new proto.AddRequest { A = 92, B = 9 })
            .ExpectError(Error.REQUEST_INVALID)
            .Run();
    }
}