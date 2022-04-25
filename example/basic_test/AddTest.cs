using NUnit.Framework;
using scyna;
using Example;

namespace test;

[TestFixture]
class AddTest
{
    [SetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/add", new AddService());
    }

    [Test]
    public void TestAddSuccess()
    {
        scyna.Test.TestService("/example/add", new example.AddRequest { A = 3, B = 5 }, new example.AddResponse { Sum = 8 }, 200);
        scyna.Test.TestService("/example/add", new example.AddRequest { A = 9, B = 12 }, new example.AddResponse { Sum = 21 }, 200);
    }

    [Test]
    public void TestAddTooBig()
    {
        scyna.Test.TestService("/example/add", new example.AddRequest { A = 90, B = 75 }, Example.Error.TOO_BIG, 400);
        scyna.Test.TestService("/example/add", new example.AddRequest { A = 92, B = 9 }, Example.Error.TOO_BIG, 400);
    }
}