namespace ex.User;
using scyna;

public class EchoService : Service.StatefulHandler<proto.CreateUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDB = dao.User.DB();
        try
        {
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
