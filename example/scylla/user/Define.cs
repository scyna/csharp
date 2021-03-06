namespace ex.User;

public class Error
{
    public static scyna.proto.Error DAO_NOT_READY = new scyna.proto.Error { Code = 100, Message = "DAO Not Ready" };
    public static scyna.proto.Error USER_NOT_EXIST = new scyna.proto.Error { Code = 101, Message = "User Not Exist" };
    public static scyna.proto.Error USER_EXIST = new scyna.proto.Error { Code = 102, Message = "User Exist" };
}

public class Path
{
    public static String CREATE_USER_URL = "/example/user/create";
    public static String GET_USER_URL = "/example/user/get";
}

public interface IUserRepository
{
    void Create(scyna.Logger LOG, User user);
    User Get(scyna.Logger LOG, ulong userID);
    User Get(scyna.Logger LOG, string email);
    bool Exist(scyna.Logger LOG, string email);
    void Update(scyna.Logger LOG, User user);
    void AddFriend(scyna.Logger LOG, ulong userID, ulong friendID);
    IEnumerable<User> ListFriend(scyna.Logger LOG, ulong userID);
}

public class Exception : System.Exception
{
    public scyna.proto.Error Error { get; }
    public Exception(scyna.proto.Error error) { this.Error = error; }
}
