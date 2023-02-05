using Google.Protobuf;
using System.Text;

namespace scyna;

public abstract class Endpoint
{
    private static JsonFormatter formater = new JsonFormatter(new JsonFormatter.Settings(true));
    public static void Register<T>(string url, Endpoint.Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Service:" + url);
        var nc = Engine.Connection;
        nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
    }

    public abstract class BaseHandler
    {
        protected Context context = new Context(0);
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
                Engine.Connection.Publish(reply, response.ToByteArray());
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
                context.Reset(request.TraceID);
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

