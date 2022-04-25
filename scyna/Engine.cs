using Google.Protobuf;
using NATS.Client;
using NATS.Client.JetStream;

namespace scyna;

public class Engine
{
    private static Engine? instance;
    private string module;
    private Session session;
    private Logger logger;
    private IConnection connection;
    private Generator id;
    private DB db;

    private IJetStream stream;
    public static Engine Instance
    {
        get
        {
            if (instance == null) throw new Exception();
            return instance;
        }
    }
    public string Module { get { return module; } }
    public IConnection Connection { get { return connection; } }
    public IJetStream Stream { get { return stream; } }

    public static DB DB { get { return Instance.db; } }
    public static Logger LOG { get { return Instance.logger; } }
    public static Generator ID { get { return Instance.id; } }
    public static ulong SessionID { get { return Instance.session.ID; } }
    private Engine(string module, ulong sid, scyna.proto.Configuration config)
    {
        this.module = module;
        session = new Session(sid);
        id = new Generator();
        logger = new Logger(sid, true);

        /* NATS */
        string[] servers = config.NatsUrl.Split(",");
        Options opts = ConnectionFactory.GetDefaultOptions();
        opts.MaxReconnect = 2;
        opts.ReconnectWait = 1000;
        opts.Servers = servers;
        if (config.NatsUsername.Length > 0)
        {
            opts.User = config.NatsUsername;
            opts.Password = config.NatsPassword;
        }
        connection = new ConnectionFactory().CreateConnection(opts);
        stream = connection.CreateJetStreamContext();
        Console.WriteLine("Connected to NATS");

        /* ScyllaDB */
        string[] hosts = config.DBHost.Split(",");
        db = DB.Init(hosts, config.DBUsername, config.DBPassword);
        Console.WriteLine("Connected to ScyllaDB");
        Console.WriteLine("Engine Created, SessionID:" + sid);
    }
    static public async void Init(string managerURL, string module, string secret)
    {
        var client = new HttpClient();
        var request = new proto.CreateSessionRequest { Module = module, Secret = secret, };
        var task = client.PostAsync(managerURL + Path.SESSION_CREATE_URL, new ByteArrayContent(request.ToByteArray()));
        if (!task.Wait(5000))
        {
            Console.WriteLine("Timeout");
            throw new Exception();
        }

        var responseBody = await task.GetAwaiter().GetResult().Content.ReadAsByteArrayAsync();
        var response = proto.CreateSessionResponse.Parser.ParseFrom(responseBody);
        instance = new Engine(module, response.SessionID, response.Config);
    }

    static public void Start()
    {
        while (true) { } //FIXME:
    }
}
