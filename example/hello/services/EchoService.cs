namespace ex.hello;

using ex.hello.dto;
using scyna;

[Endpoint("/ex/hello/echo")]
public class EchoService : Endpoint<EchoRequest>
{
    public override void Handle()
    {
        context.Info("Receive EchoRequest");
        Reply(new EchoResponse { Text = request.Text });
    }
}