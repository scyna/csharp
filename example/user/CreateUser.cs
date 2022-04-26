namespace ex.User;
using scyna;
using FluentValidation;

public class CreateUser : Service.StatefulHandler<proto.CreateUserRequest>
{
    CreateUserValidator validator = new CreateUserValidator();
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDB = dao.User.DB();
        try
        {
            validator.ValidateAndThrow(request.User);
            if (userDB.Exist(LOG, request.User.Email)) throw new dao.DBException(dao.Error.USER_EXIST);
            var user = dao.User.FromProto(request.User);
            user.ID = Engine.ID.Next();
            userDB.Create(LOG, user);
        }
        catch (dao.DBException e)
        {
            Error(e.Error);
        }
        catch (ValidationException)
        {
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