using Google.Protobuf;
using NATS.Client.JetStream;

namespace scyna;

using pb = global::Google.Protobuf;
public class Event
{
    public static void Push(string channel, pb::IMessage message)
    {
        var stream = Engine.Instance.Stream;
        stream.Publish(channel, message.ToByteArray());
    }
    public static void Register<T>(string channel, string consumer, Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Signal:" + channel);
        var stream = Engine.Instance.Stream;

        var builder = PushSubscribeOptions.Builder().WithDurable(consumer);
        stream.PushSubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
        {
            handler.Run(args.Message.Data);
        }, true, builder.Build());
    }

    public abstract class Handler<T> where T : IMessage<T>, new()
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
}
