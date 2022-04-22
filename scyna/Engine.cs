using Google.Protobuf;

namespace scyna
{
    class Engine
    {
        private static Engine? instance;
        private string module;
        private Session session;
        private Logger logger;
        public static Engine? Instance
        {
            get { return instance; }
        }
        public string Module
        {
            get { return module; }
        }
        private Engine(string module, ulong sid, scyna.proto.Configuration config)
        {
            this.module = module;
            session = new Session(sid);
            logger = new Logger(sid, true);
            /*TODO:NATS*/
            /*TODO:Scylla*/
            Console.WriteLine("Enngine Created, SessionID:" + sid);
        }
        static public async void Init(string managerURL, string module, string secret)
        {
            var client = new HttpClient();
            var request = new proto.CreateSessionRequest { Module = module, Secret = secret, };

            byte[] requestBody;
            using (MemoryStream stream = new MemoryStream())
            {
                request.WriteTo(stream);
                requestBody = stream.ToArray();
            }

            var task = client.PostAsync(managerURL + Path.SESSION_CREATE_URL, new ByteArrayContent(requestBody));
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