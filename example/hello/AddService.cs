namespace ex.hello;

using scyna;
using ex.hello.dto;

[Endpoint("/ex/hello/add")]
public class AddService : Endpoint<AddRequest>
{
    public override void Handle()
    {
        context.Info("Receive AddRequest");
        var sum = request.A + request.B;
        if (sum > 100) throw Error.REQUEST_INVALID;
        Reply(new AddResponse { Sum = sum });
    }
}