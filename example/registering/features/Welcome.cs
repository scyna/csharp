namespace ex.registering;

using scyna;

public class WelcomeHandler : DomainEvent.Handler<PROTO.RegistrationCompleted>
{
    public override void Execute()
    {
        var content = $@"Hello {data.Name}, your registration is completed.";

        context.SendRequest(Path.SEND_EMAIL, new adapter.PROTO.SendEmailRequest
        {
            Email = data.Email,
            Content = content
        });

        context.RaiseDomainEvent(new PROTO.WelcomeSent
        {
            Email = data.Email,
            Content = content
        });
    }
}