using Xunit;
using System;
using scyna;
using ex.hello;

namespace TestxUnit;

public abstract class TestsBase : IDisposable
{
    protected TestsBase()
    {
        Engine.Init("http://127.0.0.1:8081", "scyna_test", "123456");
        Endpoint.Register(Path.ECHO_URL, new EchoService());
    }

    public void Dispose()
    {
        Engine.Release();
    }
}