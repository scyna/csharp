using Google.Protobuf;
using System.Text;

namespace scyna;

public interface IEndpoint { void Run(NATS.Client.Msg message); }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EndpointAttribute : Attribute
{
    public string Url { get; }
    public EndpointAttribute(string url) { this.Url = url; }
}

public abstract class Endpoint<T> : IEndpoint where T : IMessage<T>, new()
{
    private static readonly JsonFormatter formater = new(new JsonFormatter.Settings(true));
    protected Context context = new(0);
    private bool JSON;
    private string? reply;
    private bool flushed;
    private String requestBody = "";
    protected ByteString? Metadata;

    private readonly MessageParser<T> parser = new(() => new T());
    protected T request = new();

    public abstract void Handle();

    public void Run(NATS.Client.Msg message)
    {
        try
        {
            var request = proto.Request.Parser.ParseFrom(message.Data);
            context.Reset(request.TraceID);
            reply = message.Reply;
            JSON = request.JSON;
            Metadata = request.Data;
            flushed = false;

            if (request.Body == null) throw scyna.Error.BAD_REQUEST;

            if (JSON)
            {
                requestBody = request.Body.ToString(Encoding.UTF8);
                this.request = parser.ParseJson(requestBody);
            }
            else
            {
                this.request = parser.ParseFrom(request.Body);
                requestBody = formater.Format(this.request);
            }

            this.Handle();

            if (!flushed) FlushError(200, scyna.Error.OK.ToProto());
        }
        catch (scyna.Error e)
        {
            if (e == Error.COMMAND_NOT_COMPLETED)
            {
                for (int i = 0; i < 5; i++) { if (Retry()) return; }
                FlushError(400, scyna.Error.SERVER_ERROR.ToProto());
            }
            else FlushError(400, e.ToProto());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            FlushError(400, scyna.Error.BAD_REQUEST.ToProto());
        }
    }

    private bool Retry()
    {
        try
        {
            this.Handle();
            if (!flushed) FlushError(200, scyna.Error.OK.ToProto());
        }
        catch (scyna.Error e)
        {
            if (e == Error.COMMAND_NOT_COMPLETED) return false;
            FlushError(400, e.ToProto());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            FlushError(400, scyna.Error.BAD_REQUEST.ToProto());
        }
        return true;
    }

    protected void Reply(IMessage m)
    {
        try
        {
            ByteString body;
            if (JSON) body = ByteString.CopyFrom(formater.Format(m), Encoding.UTF8);
            else body = m.ToByteString();
            var response = new proto.Response { Code = 200, Body = body };
            Engine.Connection.Publish(reply, response.ToByteArray());
        }
        catch (InvalidProtocolBufferException e)
        {
            Console.WriteLine(e.ToString());
        }
        flushed = true;
        this.Finish(200, m);
    }

    protected void FlushError(int status, proto.Error m)
    {
        try
        {
            m.Trace = (long)context.ID;
            ByteString body;
            if (JSON) body = ByteString.CopyFrom(formater.Format(m), Encoding.UTF8);
            else body = m.ToByteString();
            var response = new proto.Response { Code = status, Body = body };
            Engine.Connection.Publish(reply, response.ToByteArray());
        }
        catch (InvalidProtocolBufferException e)
        {
            Console.WriteLine(e.ToString());
        }
        flushed = true;
        this.Finish(status, m);
    }

    private void Finish(int code, IMessage response)
    {
        if (context.ID == 0)
        {
            return;
        }
        Signal.Emit(Path.ENDPOINT_DONE_CHANNEL, new proto.EndpointDoneSignal
        {
            TraceID = context.ID,
            Response = formater.Format(response),
            Request = requestBody,
            SessionID = Engine.SessionID,
            Status = code,
        });
    }

    public static void Register(string url, Endpoint<T> handler)
    {
        Console.WriteLine($"Register Service:{url}");
        var nc = Engine.Connection;
        nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
    }
}

