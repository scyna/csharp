namespace ex.registering;

using scyna;

public class ResendOtpHandler : Endpoint.Handler<PROTO.ResendOtpRequest>
{
    public override void Execute()
    {
        var row = Engine.DB.QueryOne($@"SELECT sms_count FROM {Table.REGISTRATION} WHERE email = ?", request.Email);
        if (row.GetValue<int>("sms_count") >= 5) throw Error.SMS_LIMIT_EXCEEDED;

        var otp = new Random().Next(100000, 999999).ToString();
        var expired = DateTimeOffset.Now.AddMinutes(5);

        Engine.DB.ExecuteUpdate($@"UPDATE {Table.REGISTRATION}
            SET otp=?, expired=? WHERE email=?", otp, expired, request.Email);

        context.RaiseEvent(new PROTO.OtpGenerated
        {
            Email = request.Email,
            Otp = otp
        });
    }
}
