namespace ex.User;
using scyna;
using FluentValidation;

public class GetUser : Service.Handler<proto.GetUserRequest>
{
    public override void Execute()
    {
        Console.WriteLine("Receive GetUserRequest");
        var userDB = User.Repository();
        try
        {
            var user = userDB.Get(LOG, request.Email);
            Done(new proto.GetUserResponse { User = user.ToProto() });
        }
        catch (Exception e)
        {
            Error(e.Error);
        }
    }
}
