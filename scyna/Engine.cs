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
            /*TODO*/
        }
        static public async void Init(string module, string secret)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:8081" + Path.SESSION_CREATE_URL)
            };

            var auth = new proto.CreateSessionRequest
            {
                Module = module,
                Secret = secret,
            };

            byte[] bytes;

            using (MemoryStream stream = new MemoryStream())
            {
                auth.WriteTo(stream);
                bytes = stream.ToArray();
            }

            var request = new ByteArrayContent(bytes);
            var result = await client.PostAsync("/apix/authenticate", request);
            /*TODO: timeout*/
            var response = proto.CreateSessionResponse.Parser.ParseFrom(result.Content.ReadAsStream());
            if (response != null) instance = new Engine(module, response.SessionID, response.Config);
            else throw new Exception(); //FIXME
        }
    }
}