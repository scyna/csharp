namespace ex.registering;

using scyna;
using FluentValidation;

public class RegisterUser : Endpoint.Handler<proto.RegisterUserRequest>
{
    public override void Execute()
    {
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw scyna.Error.REQUEST_INVALID;
        }

        var repository = new Repository(this.context);

        try
        {
            repository.GetUserByEmail(request.Email);
        }
        catch
        {

        }

        var user = new Account
        {
            ID = Engine.ID.Next(),
            Email = request.Email,
            Name = request.Name,
            Password = request.Password
        };

        repository.RegisterUser(user);

        context.RaiseEvent(new proto.UserRegistered
        {
            ID = Engine.ID.Next(),
            Email = request.Email,
            Name = request.Name
        });

        Response(new proto.RegisterUserResponse { ID = user.ID });
    }

    class RequestValidator : AbstractValidator<proto.RegisterUserRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(40);
            RuleFor(x => x.Password).NotEmpty().Length(6, 20);
        }
    }

    class Repository : ex.registering.Repository
    {
        public Repository(Context context) : base(context) { }
        public void RegisterUser(Account account)
        {
            /*TODO*/
        }
    }
}
