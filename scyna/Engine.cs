using Google.Protobuf;

namespace scyna
{
    class Engine
    {
        private static Engine? instance;
        private string module;
        private Session session;
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
            Console.WriteLine("Enngine Created, SessionID:" + sid);
            /*TODO:*/
        }
        static public async void Init(string managerURL, string module, string secret)
        {
            var client = new HttpClient();
            var request = new proto.CreateSessionRequest
            {
                Module = module,
                Secret = secret,
            };

            byte[] bytes;

            using (MemoryStream stream = new MemoryStream())
            {
                request.WriteTo(stream);
                bytes = stream.ToArray();
            }

            var result = client.PostAsync(managerURL + Path.SESSION_CREATE_URL, new ByteArrayContent(bytes));
            if (!result.Wait(5000))
            {
                Console.WriteLine("Timeout");
                throw new Exception();
            }

            var body = await result.GetAwaiter().GetResult().Content.ReadAsByteArrayAsync();
            var response = proto.CreateSessionResponse.Parser.ParseFrom(body);
            if (response != null) instance = new Engine(module, response.SessionID, response.Config);
            else throw new Exception();
        }
    }
}