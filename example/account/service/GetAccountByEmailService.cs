namespace ex.account;

using scyna;

public class GetAccountByEmailService : Endpoint.Handler<proto.GetAccountByEmailRequest>
{
    public override void Execute()
    {
        context.Info("Receive GetAccountByEmailRequest");
        /*TODO*/
    }
}