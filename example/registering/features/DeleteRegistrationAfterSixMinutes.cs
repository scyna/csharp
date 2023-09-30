namespace ex.registering;

using scyna;

public class WaitSixMinutes : DomainEvent<PROTO.RegistrationCreated>
{
    public override void Execute()
    {
        context.ScheduleOne(Channel.DELETE_REGISTRATION, DateTimeOffset.Now.AddMinutes(6), data);
    }
}

public class DeleteRegistrationHandler : Task.Handler<PROTO.RegistrationCreated>
{
    public override void Execute()
    {
        Engine.DB.Execute($@"DELETE FROM {Table.REGISTRATION} WHERE email = ?", data.Email);
        context.RaiseDomainEvent(new PROTO.RegistrationDeleted
        {
            Email = data.Email,
            Name = data.Name,
        });
    }
}