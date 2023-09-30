using Google.Protobuf;
using NATS.Client;
using NATS.Client.JetStream;

namespace scyna;

public class Engine
{
    private static Engine? instance;
    private readonly string module;
    private readonly Session session;
    private readonly Logger logger;
    private readonly IConnection connection;
    private readonly Settings settings;
    private readonly Generator id;
    private readonly DB db;
    private readonly IJetStream stream;
    private readonly DomainEvent domainEvent;

    public static Engine Instance
    {
        get
        {
            if (instance == null) throw new Exception();
            return instance;
        }
    }

    public static string Module { get { return Instance.module; } }
    public static IConnection Connection { get { return Instance.connection; } }
    public static Settings Settings { get { return Instance.settings; } }
    public static IJetStream Stream { get { return Instance.stream; } }
    public static DB DB { get { return Instance.db; } }
    public static DomainEvent DomainEvent { get { return Instance.domainEvent; } }
    public static Logger LOG { get { return Instance.logger; } }
    public static Generator ID { get { return Instance.id; } }
    public static ulong SessionID { get { return Instance.session.ID; } }

    private Engine(string module, ulong sid, scyna.proto.Configuration config)
    {
        this.module = module;
        session = new Session(sid);
        id = new Generator();
        logger = new Logger(sid);
        settings = new Settings();

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

        domainEvent = new DomainEvent();

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

        /*setting*/
        Signal.RegisterBySession(Path.SETTING_UPDATE_CHANNEL + module, new Settings.UpdatedSignal());
        Signal.RegisterBySession(Path.SETTING_REMOVE_CHANNEL + module, new Settings.RemovedSignal());

        /*registration*/
        RegisterEndpoints();


    }

    static public void Start()
    {
        Console.WriteLine("Engine is running");
        Event.Start();
        DomainEvent.Start();
        Console.CancelKeyPress += (_, ea) =>
        {
            ea.Cancel = true;
            Instance.Close();
            Environment.Exit(0);
        };
    }

    static public void Release()
    {
        Instance.Close();
    }

    private void Close()
    {
        connection.Close();
        db.Close();
        Console.WriteLine("Engine stopped");
    }

    static void RegisterEndpoints()
    {
        var type = typeof(IEndpoint);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

        foreach (var t in types)
        {
            EndpointAttribute attr = (EndpointAttribute)Attribute.GetCustomAttribute(t, typeof(EndpointAttribute));
            if (attr != null)
            {
                Console.WriteLine("Register endpoint: " + t.Name);
                var instance = (IEndpoint)Activator.CreateInstance(t);
                if (instance is null) throw new Exception("Can not create endpoint instance");
                Connection.SubscribeAsync(Utils.SubscribeURL(attr.Url), "API", (sender, args) => { instance.Run(args.Message); });
            }
            else
            {
                Console.WriteLine($"Class {t.Name} has no Endpoint attribute");
            }
        }
    }
}
