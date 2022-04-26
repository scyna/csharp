namespace ex.Basic.Test;

using NUnit.Framework;
using scyna;
using ex.Basic;

[TestFixture]
class AddTest
{
    [SetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/Proto/add", new AddService());
    }

    [Test]
    public void TestAddSuccess()
    {
        scyna.Test.TestService("/Proto/add", new Proto.AddRequest { A = 3, B = 5 }, new Proto.AddResponse { Sum = 8 }, 200);
        scyna.Test.TestService("/Proto/add", new Proto.AddRequest { A = 9, B = 12 }, new Proto.AddResponse { Sum = 21 }, 200);
    }

    [Test]
    public void TestAddTooBig()
    {
        scyna.Test.TestService("/Proto/add", new Proto.AddRequest { A = 90, B = 75 }, ex.Basic.Error.TOO_BIG, 400);
        scyna.Test.TestService("/Proto/add", new Proto.AddRequest { A = 92, B = 9 }, ex.Basic.Error.TOO_BIG, 400);
    }
}