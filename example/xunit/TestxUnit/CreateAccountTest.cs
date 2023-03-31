using Xunit;
using System;
using scyna;
using ex.account;

namespace TestxUnit;

public class CreateAccountTest : TestsBase
{
    [Fact]
    public void TestCreateAccount_ShouldSucess()
    {
        EndpointTest.Create(Path.CREATE_ACCOUNT_URL).
            WithRequest(new ex.account.proto.CreateAccountRequest
            {
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
                Password = "12345678",
            }).
            MatchEvent(new ex.account.proto.AccountCreated
            {
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
            }).
            ExpectSuccess().
            Run();
    }

    [Theory]
    [InlineData("a+gmail.com")]
    [InlineData("")]
    public void TestCreateAccount_ShouldReturnBadEmail(string email)
    {
        EndpointTest.Create(Path.CREATE_ACCOUNT_URL).
            WithRequest(new ex.account.proto.CreateAccountRequest
            {
                Email = email,
                Name = "Nguyen Van A",
                Password = "12345678",
            }).
            ExpectError(ex.account.Error.BAD_EMAIL).
            Run();
    }

    [Theory]
    [InlineData("")]
    [InlineData("verylongpasswordwillnotbeaccepted")]
    public void TestCreateAccount_ShouldReturnBadPassword(string password)
    {
        EndpointTest.Create(Path.CREATE_ACCOUNT_URL).
            WithRequest(new ex.account.proto.CreateAccountRequest
            {
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
                Password = password,
            }).
            ExpectError(ex.account.Error.BAD_PASSWORD).
            Run();
    }
}