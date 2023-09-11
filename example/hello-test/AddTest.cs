namespace ex.hello.Test;

using scyna;
using ex.hello;
using Xunit;

public class AddTest : TestsBase
{
    [Theory]
    [InlineData(3, 5, 8)]
    [InlineData(9, 91, 100)]
    public void TestAdd_ShouldSuccess(int a, int b, int sum)
    {
        scyna.Testing.Endpoint(ex.hello.Path.ADD_URL)
            .WithRequest(new ex.hello.proto.AddRequest { A = a, B = b })
            .ExpectResponse(new proto.AddResponse { Sum = sum })
            .ShouldBeFine();
    }

    [Theory]
    [InlineData(90, 75)]
    [InlineData(9, 92)]
    public void TestAdd_ShouldReturnTooBig(int a, int b)
    {
        scyna.Testing.Endpoint(Path.ADD_URL)
            .WithRequest(new proto.AddRequest { A = a, B = b })
            .ShouldFailWith(Error.REQUEST_INVALID);
    }
}