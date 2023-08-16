using Xunit;
using scyna;
using ex.Registering;

namespace Registering.Test;

public class GenerateOtpTest : TestBase
{
    [Fact]
    public void TestGenerateOtp_ShouldSucess()
    {
        EventTest.Create(new GenerateOtpHandler())
            .WithData(new PROTO.RegistrationCreated
            {
                ID = 12345,
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
            })
            .ExpectEvent(new PROTO.OtpGenerated
            {
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
                Otp = "123456",
            })
            .Run();
    }
}