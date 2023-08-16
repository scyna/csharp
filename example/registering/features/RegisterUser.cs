namespace ex.Registering;

using scyna;
using FluentValidation;

public class RegisterUserHandler : Endpoint.Handler<PROTO.RegisterUserRequest>
{
    record Registration
    {
        public ulong ID;
        public string? Name;
        public string? Email;
        public string? Password;
    }

    public override void Execute()
    {
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw scyna.Error.REQUEST_INVALID;
        }

        var repository = new Repository();
        repository.EmailShouldNotBeRegistered(request.Email);

        var registration = new Registration
        {
            ID = Engine.ID.Next(),
            Email = request.Email.ToLower(),
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

    class Repository
    {
        public void CreateRegistration(Registration registration)
        {
            var query = Engine.DB.Session.Prepare("INSERT INTO registering.registration(id,email,name,password) VALUES(?,?,?,?)");
            var statement = query.Bind(registration.ID, registration.Email, registration.Name, registration.Password);
            try
            {
                Engine.DB.Session.Execute(statement);
            }
            catch
            {
                throw scyna.Error.SERVER_ERROR;
            }
        }

        public void EmailShouldNotBeRegistered(string email)
        {
            var query = Engine.DB.Session.Prepare("SELECT email FROM registering.registered WHERE email=? LIMIT 1");
            var statement = query.Bind(email);
            try
            {
                Engine.DB.Session.Execute(statement).First();
                throw Error.EMAIL_REGISTERED;
            }
            catch (InvalidOperationException) { return; }
            catch
            {
                throw scyna.Error.SERVER_ERROR;
            }
        }
    }
}
