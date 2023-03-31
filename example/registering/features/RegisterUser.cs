namespace ex.registering;

using scyna;
using FluentValidation;

public class RegisterUser : Endpoint.Handler<proto.RegisterUserRequest>
{
    public override void Execute()
    {
        context.Info("Receive RegisterUserRequest");
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid) throw scyna.Error.REQUEST_INVALID;

        context.RaiseEvent(new proto.UserRegistered
        {
            ID = Engine.ID.Next(),
            Email = request.Email,
            Name = request.Name
        });
    }

    public class RequestValidator : AbstractValidator<proto.RegisterUserRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().Length(1, 40);
            RuleFor(x => x.Password).NotEmpty().Length(6, 20);
        }
    }

    public class Repository
    {
    }
}
