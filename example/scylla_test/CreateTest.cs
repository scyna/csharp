namespace ex.Scylla.Test;

using NUnit.Framework;
using ex.Scylla;
using scyna;


[TestFixture]
class CreateTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register("/example/user/create", new CreateUser());
        Service.Register("/example/user/get", new GetUser());
        dao.User.ScyllaInit();
        CleanUp();
    }

    [Test]
    public void TestCreateSuccess()
    {
        scyna.Test.TestService("/example/user/create", new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a@gmail.com",
                Password = "123456"

            }
        }, 200);

        scyna.Test.TestService("/example/user/get", new proto.GetUserRequest { Email = "a@gmail.com" }, 200);
        CleanUp();
    }

    [Test]
    public void TestCreateNoEmail()
    {
        scyna.Test.TestService("/example/user/create", new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Password = "123456"
            }
        }, scyna.Error.REQUEST_INVALID, 400);
    }

    private void CleanUp()
    {
        var session = Engine.DB.Session;
        var query = session.Prepare("TRUNCATE ex.user").Bind();
        session.Execute(query);
    }
}