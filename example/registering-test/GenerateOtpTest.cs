using Xunit;
using scyna;
using ex.registering;

namespace ex.registering.Test;

public class GenerateOtpTest : TestBase
{
    [Fact]
    public void TestGenerateOtp_ShouldSucess()
    {
        EventTest.Create(new GenerateOtpHandler())
            .WithData(new PROTO.RegistrationCreated
            {
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
            })
            .ExpectEvent(new PROTO.OtpGenerated
            {
                Email = "a@gmail.com",
                Otp = "123456",
            })
            .Run();
    }
}