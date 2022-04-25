using Google.Protobuf;

namespace scyna
{
    using pb = global::Google.Protobuf;
    class Signal
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
        public static void Register<T>(string channel, Handler<T> handler) where T : IMessage<T>, new()
        {
            Console.WriteLine("Register Signal:" + channel);
            var nc = Engine.Instance.Connection;
            nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
            {
                handler.Run(args.Message.Data);
            });
        }

        public static void Register(string channel, EmptyHandler handler)
        {
            Console.WriteLine("Register Signal:" + channel);
            var nc = Engine.Instance.Connection;
            nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) => { handler.Execute(); });
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

        public abstract class EmptyHandler
        {
            public abstract void Execute();
        }
    }
}