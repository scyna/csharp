namespace ex.account.test;

using Xunit;
using System;
using scyna;
using ex.account;

public abstract class TestsBase : IDisposable
{
    protected TestsBase()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.CREATE_ACCOUNT_URL, new CreateAccountService());
        Command.InitSingleWriter("ex_account");
    }

    public void Dispose()
    {
        cleanup();
        Engine.Release();
    }

    private void cleanup()
    {
        var statement = Engine.DB.Session.Prepare("TRUNCATE ex_account.account").Bind();
        Engine.DB.Session.Execute(statement);
    }
}