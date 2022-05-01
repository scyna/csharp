using Google.Protobuf;
using pb = global::Google.Protobuf;

namespace scyna;

public class SignalLite
{
    public static void Emit(string channel)
    {
        var msg = new proto.EventOrSignal { CallID = Engine.ID.Next() };
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, msg.ToByteArray());
    }
    public static void Register(string channel, Handler handler)
    {
        Console.WriteLine("Register Signal:" + channel);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
        {
            handler.Run(args.Message);
        });
    }
    public abstract class Handler
    {
        protected Logger LOG = new Logger(0, false);
        public void Run(NATS.Client.Msg msg)
        {
            try
            {
                var m = proto.EventOrSignal.Parser.ParseFrom(msg.Data);
                LOG.Reset(m.CallID);
                this.Execute();
            }
            catch (Exception) { }
        }
        public abstract void Execute();
    }
}
