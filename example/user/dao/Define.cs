namespace ex.User.DAO;

public class DBException : Exception
{
    public scyna.proto.Error Error { get; }
    public DBException(scyna.proto.Error error)
    {
        this.Error = error;
    }
}

public class Error
{
    public static scyna.proto.Error USER_NOT_EXIST = new scyna.proto.Error { Code = 100, Message = "User Not Exist" };
    public static scyna.proto.Error USER_EXIST = new scyna.proto.Error { Code = 101, Message = "User  Exist" };
}


public class User
{
    public ulong ID { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    private static IUserDAO DAO;
    public static void Init(IUserDAO dao)
    {
        DAO = dao;
    }
    public static IUserDAO GetDAO()
    {
        if (DAO == null) throw new Exception();
        return DAO;
    }

    public static User FromProto(Proto.User user)
    {
        return new User();
    }

    public Proto.User ToProto()
    {
        return null;
    }
}

public interface IUserDAO
{
    void Create(scyna.Logger LOG, User user);
    User Get(scyna.Logger LOG, ulong userID);
    bool Exist(scyna.Logger LOG, string email);
    void Update(scyna.Logger LOG, User user);
    void AddFriend(scyna.Logger LOG, ulong userID, ulong friendID);
    List<User> ListFriend(scyna.Logger LOG, ulong userID);
}

