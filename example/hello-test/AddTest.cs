namespace ex.hello.Test;

using scyna;
using ex.hello;
using Xunit;
using ex.hello.dto;

[Collection("Sequential")]
public class AddTest : TestsBase
{
    [Theory]
    [InlineData(3, 5, 8)]
    [InlineData(9, 91, 100)]
    public void TestAdd_ShouldSuccess(int a, int b, int sum)
    {
        Testing.Endpoint<AddService>()
            .WithRequest(new AddRequest { A = a, B = b })
            .ExpectResponse(new AddResponse { Sum = sum })
            .ExpectSucess()
            .Run();
    }

    [Theory]
    [InlineData(90, 75)]
    [InlineData(9, 92)]
    public void TestAdd_ShouldReturnTooBig(int a, int b)
    {
        Testing.Endpoint<AddService>()
            .WithRequest(new AddRequest { A = a, B = b })
            .ExpectError(Error.REQUEST_INVALID)
            .Run();
    }
}