namespace ex.User;
using scyna;

public class EchoService : Service.StatefulHandler<example.CreateUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDAO = dao.User.GetDAO();
        try
        {
            if (userDAO.Exist(LOG, request.User.Email)) throw new dao.DBException(dao.Error.USER_EXIST);
            var user = dao.User.FromProto(request.User);
            user.ID = Engine.ID.Next();
            userDAO.Create(LOG, user);
        }
        catch (dao.DBException e)
        {
            Error(e.Error);
        }
    }
}
