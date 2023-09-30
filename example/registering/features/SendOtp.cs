namespace ex.registering;

using scyna;

public class SendOtpHandler : DomainEvent<PROTO.OtpGenerated>
{
    public override void Execute()
    {
        Engine.DB.AssureExist($@"SELECT * FROM {Table.REGISTRATION} WHERE email = ?", data.Email);

        var content = $@"Your OTP is {data.Otp}. It will expire in 5 minutes.";

        context.SendRequest(Path.SEND_EMAIL, new adapter.PROTO.SendEmailRequest
        {
            Email = data.Email,
            Content = content
        });

        Engine.DB.Execute($@"UPDATE {Table.REGISTRATION}
            SET email_count = email_count + 1 WHERE email = ?", data.Email);

        context.RaiseDomainEvent(new PROTO.OtpSent
        {
            Email = data.Email,
            Otp = data.Otp,
            Content = content
        });
    }
}