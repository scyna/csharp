namespace ex.account.Test;

using NUnit.Framework;
using scyna;
using ex.account;

[TestFixture]
class CreateAccountTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.CREATE_ACCOUNT_URL, new CreateAccountService());
        Command.InitSingleWriter("ex_account");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        cleanup();
        Engine.Release();
    }

    [Test]
    public void TestCreateAccountShouldReturnSuccess()
    {
        EndpointTest.Create(Path.CREATE_ACCOUNT_URL)
                .WithRequest(new proto.CreateAccountRequest
                {
                    Email = "a@gmail.com",
                    Name = "Nguyen Van A",
                    Password = "12345678",
                })
                .PublishEventTo(Path.ACCOUNT_CREATED_CHANNEL)
                // .MatchEvent(new proto.AccountCreated
                // {
                //     Email = "a@gmail.com",
                //     Name = "Nguyen Van A",
                // })
                .ExpectSuccess()
                .Run();

        EndpointTest.Create(Path.CREATE_ACCOUNT_URL)
                .WithRequest(new proto.CreateAccountRequest
                {
                    Email = "a@gmail.com",
                    Name = "Nguyen Van A",
                    Password = "12345678",
                })
                .ExpectError(ex.account.Error.ACCOUNT_EXISTS)
                .Run();
    }

    [Test]
    public void TestCreateAccountWithBadEmail()
    {
        EndpointTest.Create(Path.CREATE_ACCOUNT_URL)
               .WithRequest(new proto.CreateAccountRequest
               {
                   Email = "a+gmail.com",
                   Name = "Nguyen Van A",
                   Password = "12345678",
               })
               .ExpectError(ex.account.Error.BAD_EMAIL)
               .Run();

        EndpointTest.Create(Path.CREATE_ACCOUNT_URL)
               .WithRequest(new proto.CreateAccountRequest
               {
                   Name = "Nguyen Van A",
                   Password = "12345678",
               })
               .ExpectError(ex.account.Error.BAD_EMAIL)
               .Run();

        EndpointTest.Create(Path.CREATE_ACCOUNT_URL)
               .WithRequest(new proto.CreateAccountRequest
               {
                   Email = "",
                   Name = "Nguyen Van A",
                   Password = "12345678",
               })
               .ExpectError(ex.account.Error.BAD_EMAIL)
               .Run();
    }

    private void cleanup()
    {
        var statement = Engine.DB.Session.Prepare("TRUNCATE ex_account.account").Bind();
        Engine.DB.Session.Execute(statement);
    }
}