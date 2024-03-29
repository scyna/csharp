using Google.Protobuf;
using pb = global::Google.Protobuf;

namespace scyna;

public class Signal
{
    public static void RegisterByModule<T>(string channel, Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Signal:" + channel);
        Engine.Connection.SubscribeAsync(channel, Engine.Module, (sender, args) =>
        {
            handler.Run(args.Message.Data);
        });
    }

    public static void RegisterBySession<T>(string channel, Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Signal:" + channel);
        Engine.Connection.SubscribeAsync(channel, (sender, args) =>
        {
            handler.Run(args.Message.Data);
        });
    }

    public static void Emit(string channel, pb::IMessage message)
    {
        Engine.Connection.Publish(channel, message.ToByteArray());
    }

    public abstract class Handler<T> where T : IMessage<T>, new()
    {
        private readonly MessageParser<T> parser = new(() => new T());
        protected T? data;
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
}