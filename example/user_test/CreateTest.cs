namespace ex.User.Test;

using NUnit.Framework;
using scyna;
using ex.User;

[TestFixture]
class CreateTest
{
    [SetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/user/create", new CreateUser());
    }

    [Test]
    public void TestCreateSuccess()
    {
    }
}