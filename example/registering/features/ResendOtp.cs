namespace ex.registering;

using scyna;

public class ResendOtpHandler : Endpoint.Handler<PROTO.ResendOtpRequest>
{
    public override void Execute()
    {
        var row = Engine.DB.QueryOne($@"SELECT email_count FROM {Table.REGISTRATION} WHERE email = ?", request.Email);
        if (row.GetValue<int>("email_count") >= 5) throw Error.SMS_LIMIT_EXCEEDED;

        var otp = Utils.GenerateSixDigitsOtp();
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
