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
    public void TestCreateThenGet()
    {
        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).ExpectSuccess().Run();

        var r = scyna.ServiceTest.New(Path.GET_USER_URL)
            .WithRequest(new proto.GetUserRequest { Email = "a@gmail.com" })
            .Run<proto.GetUserResponse>();

        Assert.AreEqual("Nguyen Van A", r.User.Name);
    }

    [Test]
    public void TestCreateNoEmail()
    {
        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Password = "123456"

                }
            }).ExpectError(scyna.Error.REQUEST_INVALID).Run();
    }

    [Test]
    public void TestCreateExisted()
    {
        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).ExpectSuccess().Run();

        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).ExpectError(ex.User.Error.USER_EXIST).Run();
    }

    [Test]
    public void TestCreateBadPassword()
    {
        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "1"

                }
            }).ExpectError(scyna.Error.REQUEST_INVALID).Run();
    }
    public void TestCreateBadEmail()
    {
        scyna.ServiceTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a+gmail.com",
                    Password = "123456"

                }
            }).ExpectError(scyna.Error.REQUEST_INVALID).Run();
    }
}