namespace ex.registering;

using scyna;

public class GenerateOtp : DomainEvent<PROTO.RegistrationCreated>
{
    public override void Execute()
    {
        var otp = Utils.GenerateSixDigitsOtp();
        var expired = DateTimeOffset.Now.AddMinutes(5);

        Engine.DB.Execute($@"UPDATE {Table.REGISTRATION}
            SET otp=?, expired=? WHERE email=?", otp, expired, data.Email);

        context.RaiseDomainEvent(new PROTO.OtpGenerated
        {
            Email = data.Email,
            Otp = otp
        });
    }
}

