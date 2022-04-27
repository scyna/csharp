namespace dao;
using Scylla.Net.Mapping;

public class User
{
    public long ID { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }

    private static IUserRepository? instance;
    public static void ScyllaInit()
    {
        MappingConfiguration.Global.Define(new Map<User>()
            .TableName("ex.user")
            .PartitionKey(u => u.ID)
            .Column(u => u.ID, cm => cm.WithName("id"))
            .Column(u => u.Email, cm => cm.WithName("email"))
            .Column(u => u.Name, cm => cm.WithName("name"))
            .Column(u => u.Password, cm => cm.WithName("password"))
        );
        instance = new UserRepository();
    }
    public static IUserRepository DB()
    {
        if (instance == null) throw new DBException(Error.DAO_NOT_READY);
        return instance;
    }

    public static User FromProto(proto.User user)
    {
        return new User
        {
            ID = (long)user.Id,
            Email = user.Email,
            Name = user.Name,
            Password = user.Password
        };
    }

    public proto.User ToProto()
    {
        return new proto.User
        {
            Id = (ulong)ID,
            Email = Email,
            Name = Name,
            Password = Password
        };
    }
}