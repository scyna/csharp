namespace ex.Basic.Test;
using NUnit.Framework;
using scyna;
using ex.Basic;

[TestFixture]
class AddTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/basic/add", new AddService());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Engine.Release();
    }

    [Test]
    public void TestAddSuccess()
    {
        scyna.ServiceTest.New(Path.ADD_USER_URL)
            .WithRequest(new proto.AddRequest { A = 3, B = 5 })
            .ExpectResponse(new proto.AddResponse { Sum = 8 })
            .Run();

        scyna.ServiceTest.New(Path.ADD_USER_URL)
            .WithRequest(new proto.AddRequest { A = 9, B = 91 })
            .ExpectResponse(new proto.AddResponse { Sum = 100 })
            .Run();
    }

    [Test]
    public void TestAddTooBig()
    {
        scyna.ServiceTest.New(Path.ADD_USER_URL)
            .WithRequest(new proto.AddRequest { A = 90, B = 75 })
            .ExpectError(ex.Basic.Error.TOO_BIG)
            .Run();
        scyna.ServiceTest.New(Path.ADD_USER_URL)
            .WithRequest(new proto.AddRequest { A = 92, B = 9 })
            .ExpectError(ex.Basic.Error.TOO_BIG)
            .Run();
    }
}