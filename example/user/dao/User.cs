namespace dao;
using ex.User;

public class User
{
    public ulong ID { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    private static IUserDAO instance;
    public static void Init(IUserDAO dao) { instance = dao; }
    public static IUserDAO Instance()
    {
        if (instance == null) throw new DBException(Error.DAO_NOT_READY);
        return instance;
    }

    public static User FromProto(proto.User user)
    {
        return new User
        {
            ID = user.Id,
            Email = user.Email,
            Name = user.Name,
            Password = user.Password
        };
    }

    public proto.User ToProto()
    {
        return new proto.User
        {
            Id = ID,
            Email = Email,
            Name = Name,
            Password = Password
        };
    }
}
