using Google.Protobuf;
using NATS.Client;

namespace scyna
{
    class Engine
    {
        private static Engine? instance;
        private string module;
        private Session session;
        private Logger logger;
        private IConnection connection;
        private Generator id;
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

        public static Logger LOG { get { return Instance.logger; } }
        public static Generator ID { get { return Instance.id; } }
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
            Console.WriteLine("Connected to NATS");

            /* ScyllaDB */
            /*TODO*/

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
            if (response != null) instance = new Engine(module, response.SessionID, response.Config);
            else throw new Exception();
        }
    }
}