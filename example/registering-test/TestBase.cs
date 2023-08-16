namespace ex.registering.Test;

using System;
using scyna;
using ex.registering;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.CREATE_REGISTRATION, new CreateRegistrationHandler());
    }

    public void Dispose()
    {
        cleanup();
        Engine.Release();
    }

    private void cleanup()
    {
        //var statement = Engine.DB.Session.Prepare("TRUNCATE ex_registering.user").Bind();
        //Engine.DB.Session.Execute(statement);
    }
}