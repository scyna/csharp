namespace scyna;

using Google.Protobuf;
using System.Text;

class Request
{
    public static proto.Response? Send(string url, IMessage? request)
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

    public static T? Send<T>(string url, IMessage? request) where T : IMessage<T>, new()
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
}