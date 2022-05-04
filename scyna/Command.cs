using Google.Protobuf;
using System.Text;

namespace scyna;

public abstract class Command
{
    public static void Register(string url, Handler handler)
    {
        Console.WriteLine("Register Command:" + url);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(Utils.SubscribeURL(url), "API", (sender, args) => { handler.Run(args.Message); });
    }

    public static proto.Response? Send(string url)
    {
        return Service.SendRequest(url, null);
    }

    public static T? Send<T>(string url) where T : IMessage<T>, new()
    {
        return Service.SendRequest<T>(url, null);
    }

    public abstract class Handler : Service.BaseHandler
    {
        public abstract void Execute();
        public void Run(NATS.Client.Msg message)
        {
            try
            {
                var request = proto.Request.Parser.ParseFrom(message.Data);
                LOG.Reset(request.CallID);
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
}
