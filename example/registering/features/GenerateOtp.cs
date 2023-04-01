namespace Registering;


using scyna;
using FluentValidation;

public class GenerateOtpHandler : DomainEvent.Handler<PROTO.RegistrationCreated>
{
    public override void Execute()
    {
        context.RaiseEvent(new PROTO.OtpGenerated
        {
            ID = data.ID,
            Email = data.Email,
            Name = data.Name,
            Otp = "123456"
        });
    }
}