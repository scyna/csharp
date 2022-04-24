using Scylla.Net;
namespace scyna
{
    class DB
    {
        private Cluster cluster;
        private ISession session;

        private DB(String[] hosts, string username, string password)
        {
            var builder = Cluster.Builder().AddContactPoints(hosts);
            if (username.Length > 0) builder.WithCredentials(username, password);
            cluster = builder.Build();
            session = cluster.Connect();
        }
        public ISession Session { get { return session; } }

        public static DB Init(String[] hosts, String username, String password)
        {
            return new DB(hosts, username, password);
        }
    }
}