namespace ex.registering;

using Cassandra;
using scyna;

public class VerifyRegistrationHandler : Endpoint.Handler<PROTO.VerifyRegistrationRequest>
{
    public override void Execute()
    {
        var auth = Engine.DB.QueryOne($@"SELECT name,password,otp,expired FROM {Table.REGISTRATION}
            WHERE email = ?", request.Email);

        var expired = auth.GetValue<DateTimeOffset>("expired");
        if (expired > DateTimeOffset.Now) throw Error.OTP_EXPIRED;

        Engine.DB.ExecuteUpdate(new BatchStatement()
            .Add($@"DELETE FROM {Table.REGISTRATION} WHERE email = ?", request.Email)
            .Add($@"INSERT INTO {Table.COMPLETED} (email) VALUES (?)", request.Email));

        context.PublishEvent(Channel.REGISTRATION_COMPLETED, new PROTO.RegistrationCompleted
        {
            Email = request.Email,
            Name = auth.GetValue<string>("name"),
            Password = auth.GetValue<string>("password"),
        });
    }
}
