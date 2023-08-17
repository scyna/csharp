namespace ex.registering;

using scyna;

public class WaitSixMinutesHandler : DomainEvent.Handler<PROTO.RegistrationCreated>
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
        Engine.DB.ExecuteUpdate($@"DELETE FROM {Table.REGISTRATION} WHERE email = ?", data.Email);
        context.RaiseEvent(new PROTO.RegistrationDeleted
        {
            Email = data.Email,
            Name = data.Name,
        });
    }
}