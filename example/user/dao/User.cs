namespace dao;
using Scylla.Net.Mapping;

public class User
{
    public ulong ID { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }

    private static IUserDB? instance;
    public static void ScyllaInit()
    {
        MappingConfiguration.Global.Define(new Map<User>()
            .TableName("users")
            .PartitionKey(u => u.ID)
            .Column(u => u.ID, cm => cm.WithName("id"))
            .Column(u => u.Email, cm => cm.WithName("email"))
            .Column(u => u.Name, cm => cm.WithName("name"))
            .Column(u => u.Password, cm => cm.WithName("password"))
        );
        instance = new UserDB();
    }
    public static IUserDB DB()
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
