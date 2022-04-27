
namespace scyna;
using Scylla.Net;
using Scylla.Net.Mapping;

public class DB
{
    private Cluster cluster;
    private ISession session;
    private IMapper mapper;

    private DB(String[] hosts, string username, string password)
    {
        var builder = Cluster.Builder().AddContactPoints(hosts);
        if (username.Length > 0) builder.WithCredentials(username, password);
        cluster = builder.Build();
        session = cluster.Connect();
        mapper = new Mapper(session);
    }
    public ISession Session { get { return session; } }
    public IMapper Mapper { get { return mapper; } }

    public static DB Init(String[] hosts, String username, String password)
    {
        return new DB(hosts, username, password);
    }
    public void Close()
    {
        cluster.Shutdown();
    }
}
