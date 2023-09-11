namespace ex.registering.Test;

using System;
using scyna;
using ex.registering;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Registration.TestingSetup();
    }

    public void Dispose()
    {
        Cleanup();
        Engine.Release();
    }

    private void Cleanup()
    {
        Engine.DB.Execute($"TRUNCATE {Table.REGISTRATION}");
        Engine.DB.Execute($"TRUNCATE {Table.COMPLETED}");
    }
}