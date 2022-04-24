using Google.Protobuf;

namespace scyna
{
    using pb = global::Google.Protobuf;
    class Signal
    {
        public static void Emit(String channel)
        {
            var nc = Engine.Instance.Connection;
            nc.Publish(channel, null);
        }

        public static void Emit(String channel, pb::IMessage message)
        {
            var nc = Engine.Instance.Connection;
            nc.Publish(channel, message.ToByteArray());
        }
        public static void Register<T>(String channel, Handler<T> handler) where T : IMessage<T>, new()
        {
            Console.WriteLine("Register Signal:" + channel);
            var nc = Engine.Instance.Connection;
            var d = nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
            {
                handler.Run(args.Message.Data);
            });
        }

        public abstract class Handler<T> where T : IMessage<T>, new()
        {
            private MessageParser<T> parser = new MessageParser<T>(() => new T());
            public abstract void Execute(T data);
            public void Run(byte[] data)
            {
                try
                {
                    Console.WriteLine("Here");
                    Execute(parser.ParseFrom(data));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

    }
}