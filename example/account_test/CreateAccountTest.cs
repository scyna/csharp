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
        /*TODO*/
    }

    [Test]
    public void TestCreateAccountWithWrongEmail()
    {
        /*TODO*/
    }

    private void cleanup()
    {
        /*TODO*/
    }
}