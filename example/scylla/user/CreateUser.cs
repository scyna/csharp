namespace ex.User;
using FluentValidation;

public class CreateUser : scyna.Service.Handler<proto.CreateUserRequest>
{
    CreateUserValidator validator = new CreateUserValidator();
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDB = User.Repository();
        try
        {
            validator.ValidateAndThrow(request.User);

            if (userDB.Exist(LOG, request.User.Email)) throw new Exception(ex.User.Error.USER_EXIST);
            var user = User.FromProto(request.User);
            user.ID = (long)scyna.Engine.ID.Next();
            userDB.Create(LOG, user);
            Done(new proto.CreateUserResponse { Id = (ulong)user.ID });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Error.Message);
            Error(e.Error);
        }
        catch (ValidationException)
        {
            Console.WriteLine("Request not valid");
            Error(scyna.Error.REQUEST_INVALID);
        }
    }
}
public class CreateUserValidator : AbstractValidator<proto.User>
{
    public CreateUserValidator()
    {
        RuleFor(u => u.Name).NotNull();
        RuleFor(u => u.Email).NotNull().EmailAddress();
        RuleFor(u => u.Password).NotNull().Length(5, 10);
    }
}