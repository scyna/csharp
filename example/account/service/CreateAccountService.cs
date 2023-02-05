namespace ex.account;

using scyna;

public class CreateAccountService : Endpoint.Handler<proto.CreateAccountRequest>
{
    public override void Execute()
    {
        LOG.Info("Receive CreateAccountRequest");
        /*TODO*/
    }
}