namespace ex.account;

using scyna;

public class GetAccountByEmailService : Endpoint.Handler<proto.GetAccountByEmailRequest>
{
    public override void Execute()
    {
        LOG.Info("Receive GetAccountByEmailRequest");
        /*TODO*/
    }
}