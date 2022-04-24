using Google.Protobuf;
using System.Text;

namespace scyna
{
    abstract class Service
    {
        private static JsonFormatter formater = new JsonFormatter(new JsonFormatter.Settings(true));
        public static void Register<T>(string url, Service.Handler<T> handler) where T : IMessage<T>, new()
        {
            Console.WriteLine("Register Service:" + url);
            var nc = Engine.Instance.Connection;
            var d = nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
        }

        public static void Register(string url, Service.EmptyHandler handler)
        {
            Console.WriteLine("Register Service:" + url);
            var nc = Engine.Instance.Connection;
            var d = nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
        }

        public static proto.Response? SendRequest(string url, IMessage? request)
        {
            var callID = Engine.ID.next();
            var req = new proto.Request { CallID = callID, JSON = false };
            if (request != null) req.Body = request.ToByteString();
            try
            {
                var msg = Engine.Instance.Connection.Request(Utils.PublishURL(url), req.ToByteArray(), 10000);
                return proto.Response.Parser.ParseFrom(msg.Data);
            }
            catch (Exception) { return null; }
        }

        public static T? SendRequest<T>(string url, IMessage? request) where T : IMessage<T>, new()
        {
            var callID = Engine.ID.next();
            var req = new proto.Request { CallID = callID, JSON = false };
            if (request != null) req.Body = request.ToByteString();
            try
            {
                var msg = Engine.Instance.Connection.Request(Utils.PublishURL(url), req.ToByteArray(), 10000);
                var response = proto.Response.Parser.ParseFrom(msg.Data);
                if (response.Code != 200) return default(T);
                MessageParser<T> parser = new MessageParser<T>(() => new T());
                return parser.ParseFrom(response.Body);
            }
            catch (Exception) { return default(T); }
        }

        public abstract class BaseHandler
        {
            protected Logger LOG = new Logger(0, false);
            protected bool JSON;
            protected string? source;
            protected string? reply;
            protected void Error(proto.Error error)
            {
                flush(400, error);
            }
            protected void Done(IMessage m)
            {
                flush(200, m);
            }
            protected void flush(int status, IMessage m)
            {
                try
                {
                    ByteString body;
                    if (JSON) body = ByteString.CopyFrom(formater.Format(m), Encoding.ASCII);
                    else body = m.ToByteString();
                    var response = new proto.Response { Code = status, SessionID = Engine.SessionID, Body = body };
                    Engine.Instance.Connection.Publish(reply, response.ToByteArray());
                }
                catch (InvalidProtocolBufferException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public abstract class EmptyHandler : BaseHandler
        {
            public abstract void Execute();
            public void Run(NATS.Client.Msg message)
            {
                try
                {
                    var request = proto.Request.Parser.ParseFrom(message.Data);
                    LOG.Reset(request.CallID, true); // FIXME: 
                    reply = message.Reply;
                    JSON = request.JSON;
                    source = request.Data;
                    Execute();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    flush(400, scyna.Error.BAD_REQUEST);
                }
            }
        }

        public abstract class Handler<T> : BaseHandler where T : IMessage<T>, new()
        {
            private MessageParser<T> parser = new MessageParser<T>(() => new T());
            public abstract void Execute(T request);
            public void Run(NATS.Client.Msg message)
            {
                try
                {
                    var request = proto.Request.Parser.ParseFrom(message.Data);
                    LOG.Reset(request.CallID, true); // FIXME: 
                    reply = message.Reply;
                    JSON = request.JSON;
                    source = request.Data;

                    if (request.Body == null) throw new Exception();
                    if (JSON) Execute(parser.ParseJson(request.Body.ToString(Encoding.ASCII)));
                    else Execute(parser.ParseFrom(request.Body));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    flush(400, scyna.Error.BAD_REQUEST);
                }
            }
        }
    }
}