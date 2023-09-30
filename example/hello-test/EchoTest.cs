namespace ex.hello.Test;

using Xunit;
using scyna;
using ex.hello;
using ex.hello.dto;

[Collection("Sequential")]
public class EchoTest : TestsBase
{
    [Fact]
    public void TestEchoSuccess()
    {
        Testing.Endpoint<EchoService>()
            .WithRequest(new EchoRequest { Text = "Hello" })
            .ExpectResponse(new EchoResponse { Text = "Hello" })
            .Run();
    }
}