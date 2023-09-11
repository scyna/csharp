namespace ex.registering;

using scyna;
using FluentValidation;

public class CreateRegistrationHandler : Endpoint.Handler<PROTO.CreateRegistrationRequest>
{
    public override void Execute()
    {
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid) throw scyna.Error.REQUEST_INVALID;

        Engine.DB.AssureNotExist($@"SELECT * FROM {Table.REGISTRATION} WHERE email = ?", request.Email);
        Engine.DB.AssureNotExist($@"SELECT * FROM {Table.COMPLETED} WHERE email = ?", request.Email);

        Engine.DB.Execute($@"INSERT INTO {Table.REGISTRATION} (email,name,password)
            VALUES (?, ?, ?)", request.Email, request.Name, request.Password);

        context.RaiseDomainEvent(new PROTO.RegistrationCreated
        {
            Email = request.Email,
            Name = request.Name,
            Password = request.Password,
        });
    }

    class RequestValidator : AbstractValidator<PROTO.CreateRegistrationRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(40);
            RuleFor(x => x.Password).NotEmpty().Length(6, 20);
        }
    }
}
