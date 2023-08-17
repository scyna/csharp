namespace ex.registering;

using scyna;

public class DeleteRegistrationAfterSixMinutesHandler : DomainEvent.Handler<PROTO.RegistrationCreated>
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