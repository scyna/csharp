using Google.Protobuf;
using System.Text;

namespace scyna
{
    abstract class Service
    {
        protected Logger log = new Logger(0, false);
        protected bool json;
        protected ByteString? requestBody;
        protected string? requestSource;
        protected string? reply;
        private static JsonFormatter formater = new JsonFormatter(new JsonFormatter.Settings(false));

        public abstract void Execute();

        public static void register(string url, Service service)
        {
            Console.WriteLine("Register Service:" + url);
            var nc = Engine.Instance.Connection;
            var d = nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) =>
            {
                try
                {
                    var request = proto.Request.Parser.ParseFrom(args.Message.Data);
                    service.log.Reset(request.CallID, true); // FIXME: 
                    service.reply = args.Message.Reply;
                    service.json = request.JSON;
                    service.requestBody = request.Body;
                    service.requestSource = request.Data;
                    service.Execute();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });
        }

        public static proto.Response? sendRequest(string url, IMessage request)
        {
            var callID = Engine.ID.next();
            var req = new proto.Request { CallID = callID, JSON = false };
            if (request != null) req.Body = request.ToByteString();

            try
            {
                var msg = Engine.Instance.Connection.Request(Utils.PublishURL(url), request.ToByteArray(), 10000);
                return proto.Response.Parser.ParseFrom(msg.Data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void error(proto.Error error)
        {
            flush(400, error);
        }

        protected void done(IMessage m)
        {
            flush(200, m);
        }

        protected void flush(int status, IMessage m)
        {
            try
            {
                ByteString body;
                if (json) body = ByteString.CopyFrom(formater.Format(m), Encoding.ASCII);
                else body = m.ToByteString();
                var response = new proto.Response { Code = status, SessionID = Engine.SessionID, Body = body };
                Engine.Instance.Connection.Publish(reply, response.ToByteArray());
            }
            catch (InvalidProtocolBufferException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public abstract class Base<T> : Service where T : IMessage<T>, new()
        {
            private MessageParser<T> parser = new MessageParser<T>(() => new T());
            protected T? parse()
            {
                try
                {
                    if (requestBody == null) throw new Exception();
                    if (json) return parser.ParseJson(requestBody.ToString(Encoding.ASCII));
                    else return parser.ParseFrom(requestBody);
                }
                catch (Exception)
                {
                    flush(400, Error.BAD_REQUEST);
                    return default(T);
                }
            }
        }
    }
}