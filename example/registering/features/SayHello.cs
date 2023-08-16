namespace ex.registering;

using scyna;

public class SayHelloHandler : DomainEvent.Handler<PROTO.RegistrationCompleted>
{
    public override void Execute()
    {
        var content = $@"Hello {data.Name}, your registration is completed.";
        /*TODO: send email*/
    }
}