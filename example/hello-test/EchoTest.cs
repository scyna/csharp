namespace ex.hello.Test;

using Xunit;
using scyna;
using ex.hello;

public class EchoTest : TestsBase
{
    [Fact]
    public void TestEchoSuccess()
    {
        scyna.Testing.Endpoint(Path.ECHO_URL)
            .WithRequest(new proto.EchoRequest { Text = "Hello" })
            .ExpectResponse(new proto.EchoResponse { Text = "Hello" })
            .ShouldBeFine();
    }
}