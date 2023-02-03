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
        Endpoint.Register(Path.CREATE_USER_URL, new CreateUser());
        Endpoint.Register(Path.GET_USER_URL, new GetUser());
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
        Engine.DB.Session.Execute("TRUNCATE ex.user");
    }

    [Test]
    public void TestCreateSuccess()
    {
        scyna.EndpointTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).ExpectSuccess().Run();
    }

    [Test]
    public void TestCreateThenGet()
    {
        var r = scyna.EndpointTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).Run<proto.CreateUserResponse>();

        scyna.EndpointTest.New(Path.GET_USER_URL)
            .WithRequest(new proto.GetUserRequest { Email = "a@gmail.com" })
            .ExpectResponse(new proto.GetUserResponse
            {
                User = new proto.User
                {
                    Id = r.Id,
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            })
            .Run();
    }

    [Test]
    public void TestCreateNoEmail()
    {
        scyna.EndpointTest.New(Path.CREATE_USER_URL)
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
        scyna.EndpointTest.New(Path.CREATE_USER_URL)
            .WithRequest(new proto.CreateUserRequest
            {
                User = new proto.User
                {
                    Name = "Nguyen Van A",
                    Email = "a@gmail.com",
                    Password = "123456"

                }
            }).ExpectSuccess().Run();

        scyna.EndpointTest.New(Path.CREATE_USER_URL)
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
        scyna.EndpointTest.New(Path.CREATE_USER_URL)
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
        scyna.EndpointTest.New(Path.CREATE_USER_URL)
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