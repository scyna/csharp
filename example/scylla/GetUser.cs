namespace ex.Scylla;
using scyna;
using FluentValidation;

public class GetUser : Service.StatefulHandler<proto.GetUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive GetUserRequest");
        var userDB = db.User.Repository();
        try
        {
            var user = userDB.Get(LOG, request.Email);
            Done(new proto.GetUserResponse { User = user.ToProto() });
        }
        catch (db.Exception e)
        {
            Error(e.Error);
        }
    }
}
