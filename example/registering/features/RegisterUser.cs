namespace Registering;

using scyna;
using FluentValidation;

public class RegisterUserHandler : Endpoint.Handler<PROTO.RegisterUserRequest>
{
    public override void Execute()
    {
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw scyna.Error.REQUEST_INVALID;
        }

        var repository = new Repository(this.context);
        repository.EmailShouldNotBeRegistered(request.Email);

        var registration = new Registration
        {
            ID = Engine.ID.Next(),
            Email = request.Email,
            Name = request.Name,
            Password = request.Password
        };

        repository.CreateRegistration(registration);
        context.RaiseEvent(new PROTO.RegistrationCreated
        {
            ID = registration.ID,
            Email = registration.Email,
            Name = registration.Name
        });

        Response(new PROTO.RegisterUserResponse { ID = registration.ID });
    }

    class RequestValidator : AbstractValidator<PROTO.RegisterUserRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(40);
            RuleFor(x => x.Password).NotEmpty().Length(6, 20);
        }
    }

    class Repository : RepositoryBase
    {
        public Repository(Context context) : base(context) { }
        public void CreateRegistration(Registration account)
        {
            /*TODO*/
        }

        public void EmailShouldNotBeRegistered(string email)
        {
            try
            {
                GetUserByEmail(email);
                throw Error.USER_EXISTS;
            }
            catch (scyna.Error err)
            {
                if (err == scyna.Error.SERVER_ERROR) throw;
            }
        }
    }
}
