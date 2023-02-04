using Google.Protobuf;
using System.Text;

namespace scyna;

public abstract class Endpoint
{
    private static JsonFormatter formater = new JsonFormatter(new JsonFormatter.Settings(true));
    public static void Register<T>(string url, Endpoint.Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Service:" + url);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
    }

    public static proto.Response? SendRequest(string url, IMessage? request)
    {
        var traceID = Engine.ID.Next();
        var req = new proto.Request { TraceID = traceID, JSON = false };
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
        var traceID = Engine.ID.Next();
        var req = new proto.Request { TraceID = traceID, JSON = false };
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
        protected bool flushed;

        protected void Response(IMessage m)
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
            flushed = true;
        }
    }

    public abstract class Handler<T> : BaseHandler where T : IMessage<T>, new()
    {
        private MessageParser<T> parser = new MessageParser<T>(() => new T());
        protected T request = new T();

        public abstract void Execute();

        public void Run(NATS.Client.Msg message)
        {
            try
            {
                var request = proto.Request.Parser.ParseFrom(message.Data);
                LOG.Reset(request.TraceID);
                reply = message.Reply;
                JSON = request.JSON;
                source = request.Data;
                flushed = false;

                if (request.Body == null) throw scyna.Error.BAD_REQUEST;

                if (JSON) this.request = parser.ParseJson(request.Body.ToString(Encoding.ASCII));
                else this.request = parser.ParseFrom(request.Body);

                this.Execute();

                if (!flushed) flush(200, scyna.Error.OK.ToProto());
            }
            catch (scyna.Error e)
            {
                Console.WriteLine(e.ToString());
                flush(400, e.ToProto());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                flush(400, scyna.Error.BAD_REQUEST.ToProto());
            }
        }
    }
}
