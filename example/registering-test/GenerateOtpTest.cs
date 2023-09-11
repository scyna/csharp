using Xunit;
using scyna;
using ex.registering;

namespace ex.registering.Test;

public class GenerateOtpTest : TestBase
{
    [Fact]
    public void TestGenerateOtp_ShouldSucess()
    {
        Testing.DomainEvent(new GenerateOtpHandler())
            .WithData(new PROTO.RegistrationCreated { Email = "a@gmail.com", Name = "Nguyen Van A" })
            .ExpectDomainEvent(new PROTO.OtpGenerated { Email = "a@gmail.com", Otp = "123456" })
            .ShouldBeFine();
    }
}