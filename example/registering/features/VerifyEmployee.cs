namespace ex.registering;

using Cassandra;
using scyna;

public class VerifyRegistrationHandler : Endpoint.Handler<PROTO.VerifyRegistrationRequest>
{
    public override void Execute()
    {
        var row = Engine.DB.QueryOne($@"SELECT name,password,otp,expired FROM {Table.REGISTRATION}
            WHERE email = ?", request.Email);

        var expired = row.GetValue<DateTimeOffset>("expired");
        if (expired > DateTimeOffset.Now) throw Error.OTP_EXPIRED;

        Engine.DB.Execute(new BatchStatement()
            .Add($@"DELETE FROM {Table.REGISTRATION} WHERE email = ?", request.Email)
            .Add($@"INSERT INTO {Table.COMPLETED} (email, state) VALUES (?,?)", request.Email, EmailState.ACTIVE));

        context.PublishEvent(Channel.REGISTRATION_COMPLETED, new PROTO.RegistrationCompleted
        {
            Email = request.Email,
            Name = row.GetValue<string>("name"),
            Password = row.GetValue<string>("password"),
        });
    }
}
