using Xunit;
using scyna;
using Registering;

namespace Registering.Test;

public class GenerateOtpTest : TestBase
{
    [Fact]
    public void TestGenerateOtp_ShouldSucess()
    {
        EventTest.Create(new GenerateOtpHandler())
            .ExpectEvent(new PROTO.OtpGenerated
            {
                //ID = 12345,
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
                Otp = "123456",
            })
            .Run(new PROTO.RegistrationCreated
            {
                ID = 12345,
                Email = "a@gmail.com",
                Name = "Nguyen Van A",
            });
    }
}