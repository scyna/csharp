namespace ex.User;
using scyna;

public class EchoService : Service.StatefulHandler<Proto.CreateUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive CreateUserRequest");
        var userDAO = DAO.User.GetDAO();
        try
        {
            if (userDAO.Exist(LOG, request.User.Email)) throw new DAO.DBException(DAO.Error.USER_EXIST);
            var user = DAO.User.FromProto(request.User);
            user.ID = Engine.ID.Next();
            userDAO.Create(LOG, user);
        }
        catch (DAO.DBException e)
        {
            Error(e.Error);
        }
    }
}
