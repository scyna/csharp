namespace ex.hello;

using scyna;

public class AddService : Endpoint.Handler<proto.AddRequest>
{
    public override void Execute()
    {
        context.Info("Receive AddRequest");

        var sum = request.A + request.B;
        if (sum > 100) throw scyna.Error.REQUEST_INVALID;

        Response(new proto.AddResponse { Sum = sum });

    }
}