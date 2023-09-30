namespace ex.hello;

using scyna;

[Endpoint("/ex/hello/echo")]
public class EchoService : Endpoint<proto.EchoRequest>
{
    public override void Handle()
    {
        context.Info("Receive EchoRequest");
        Reply(new proto.EchoResponse { Text = request.Text });
    }
}