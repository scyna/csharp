namespace ex.registering;

using scyna;

public class GenerateOtpHandler : DomainEvent.Handler<PROTO.RegistrationCreated>
{
    public override void Execute()
    {
        var otp = new Random().Next(100000, 999999).ToString();
        Engine.DB.ExecuteUpdate($@"UPDATE {Table.REGISTRATION}
            SET otp=? WHERE email=?", otp, data.Email);
        context.RaiseEvent(new PROTO.OtpGenerated
        {
            Email = data.Email,
            Otp = otp,
        });
    }
}