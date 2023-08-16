namespace ex.registering;

using scyna;

public class SendOtpHandler : DomainEvent.Handler<PROTO.OtpGenerated>
{
    public override void Execute()
    {
        Engine.DB.AssureExist($@"SELECT email FROM {Table.REGISTRATION} WHERE email = ?", data.Email);

        var content = $@"Your OTP is {data.Otp}. It will expire in 5 minutes.";

        /*TODO: send sms*/

        Engine.DB.ExecuteUpdate($@"UPDATE {Table.REGISTRATION}
            SET sms_count = sms_count + 1 WHERE email = ?", data.Email);

        context.RaiseEvent(new PROTO.OtpSent
        {
            Email = data.Email,
            Otp = data.Otp,
            Content = content
        });
    }
}