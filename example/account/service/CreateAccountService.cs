namespace ex.account;

using scyna;

public class CreateAccountService : Endpoint.Handler<proto.CreateAccountRequest>
{
    public override void Execute()
    {
        context.Info("Receive CreateAccountRequest");
        /*TODO*/
    }
}