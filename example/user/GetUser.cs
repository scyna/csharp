namespace ex.User;
using scyna;
using FluentValidation;

public class GetUser : Service.StatefulHandler<proto.GetUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive GetUserRequest");
        var userDB = dao.User.DB();
        try
        {
            var user = userDB.Get(LOG, request.Email);
            Done(new proto.GetUserResponse { User = user.ToProto() });
        }
        catch (dao.DBException e)
        {
            Error(e.Error);
        }
    }
}
