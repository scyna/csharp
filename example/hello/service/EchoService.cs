namespace ex.hello;

using scyna;

public class EchoService : Endpoint.Handler<proto.EchoRequest>
{
    public override void Execute()
    {
        LOG.Info("Receive EchoRequest");
        Response(new proto.EchoResponse { Text = request.Text });
    }
}