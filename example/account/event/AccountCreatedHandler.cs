namespace ex.account;

using scyna;

public class AccountCreatedHandler : Event.Handler<proto.AccountCreated>
{
    public override void Execute()
    {
        context.Info("Receive AccountCreated event");
        Console.WriteLine(data);
        /* TODO: do something here*/
    }
}