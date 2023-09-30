namespace ex.hello;

using scyna;

[Endpoint("/ex/hello/add")]
public class AddService : Endpoint<proto.AddRequest>
{
    public override void Handle()
    {
        context.Info("Receive AddRequest");
        var sum = request.A + request.B;
        if (sum > 100) throw scyna.Error.REQUEST_INVALID;
        Reply(new proto.AddResponse { Sum = sum });
    }
}