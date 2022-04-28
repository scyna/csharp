namespace ex.UserTest;

using NUnit.Framework;
using ex.User;
using scyna;


[TestFixture]
class CreateTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
        Service.Register(Path.CREATE_USER_URL, new CreateUser());
        Service.Register(Path.GET_USER_URL, new GetUser());
        ex.User.User.ScyllaInit();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        CleanUp();
        Engine.Release();
    }

    [SetUp]
    public void CleanUp()
    {
        var session = Engine.DB.Session;
        var query = session.Prepare("TRUNCATE ex.user").Bind();
        session.Execute(query);
    }

    [Test]
    public void TestCreateSuccess()
    {
        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a@gmail.com",
                Password = "123456"

            }
        }, 200);
        scyna.Test.TestService(Path.GET_USER_URL, new proto.GetUserRequest { Email = "a@gmail.com" }, 200);
    }

    [Test]
    public void TestCreateNoEmail()
    {
        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Password = "123456"
            }
        }, scyna.Error.REQUEST_INVALID, 400);
    }

    [Test]
    public void TestCreateExisted()
    {
        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a@gmail.com",
                Password = "123456"

            }
        }, 200);

        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a@gmail.com",
                Password = "123456"

            }
        }, ex.User.Error.USER_EXIST, 400);
    }

    [Test]
    public void TestCreateBadPassword()
    {
        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a@gmail.com",
                Password = "1"

            }
        }, scyna.Error.REQUEST_INVALID, 400);
    }
    public void TestCreateBadEmail()
    {
        scyna.Test.TestService(Path.CREATE_USER_URL, new proto.CreateUserRequest
        {
            User = new proto.User
            {
                Name = "Nguyen Van A",
                Email = "a+gmail.com",
                Password = "123456"

            }
        }, scyna.Error.REQUEST_INVALID, 400);
    }

}