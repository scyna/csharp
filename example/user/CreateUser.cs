namespace ex.User;
using scyna;
using FluentValidation;

public class CreateUser : Service.StatefulHandler<proto.CreateUserRequest>
{
    UserValidator validator = new UserValidator();
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDB = dao.User.DB();
        try
        {
            if (!validator.Validate(request.User).IsValid) throw new dao.DBException(scyna.Error.REQUEST_INVALID);
            if (userDB.Exist(LOG, request.User.Email)) throw new dao.DBException(dao.Error.USER_EXIST);
            var user = dao.User.FromProto(request.User);
            user.ID = Engine.ID.Next();
            userDB.Create(LOG, user);
        }
        catch (dao.DBException e)
        {
            Error(e.Error);
        }
    }
}

public class UserValidator : AbstractValidator<proto.User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name).NotNull();
        RuleFor(u => u.Email).NotNull().EmailAddress();
        RuleFor(u => u.Password).NotNull().Length(5, 10);
    }
}