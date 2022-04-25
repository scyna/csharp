using Google.Protobuf;
using pb = global::Google.Protobuf;

namespace scyna;

public class Signal
{
    public static void Emit(string channel)
    {
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, null);
    }

    public static void Emit(string channel, pb::IMessage message)
    {
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, message.ToByteArray());
    }
    public static void Register<T>(string channel, StatefulHandler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Signal:" + channel);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
        {
            handler.Run(args.Message.Data);
        });
    }

    public static void Register(string channel, StatelessHandler handler)
    {
        Console.WriteLine("Register Signal:" + channel);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) => { handler.Execute(); });
    }

    public abstract class StatefulHandler<T> where T : IMessage<T>, new()
    {
        private MessageParser<T> parser = new MessageParser<T>(() => new T());
        protected T data;
        public abstract void Execute();
        public void Run(byte[] data)
        {
            try
            {
                this.data = parser.ParseFrom(data);
                Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public abstract class StatelessHandler
    {
        public abstract void Execute();
    }
}
