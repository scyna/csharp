using Google.Protobuf;

namespace scyna
{
    using pb = global::Google.Protobuf;
    class Signal
    {
        public interface Handler
        {
            void execute(byte[] data);
        }

        public static void Emit(String channel)
        {
            var nc = Engine.Instance.Connection;
            nc.Publish(channel, null);
        }

        public static void Emit(String channel, pb::IMessage message)
        {
            var nc = Engine.Instance.Connection;
            byte[] data;
            using (MemoryStream stream = new MemoryStream())
            {
                message.WriteTo(stream);
                data = stream.ToArray();
            }
            nc.Publish(channel, data);
        }
        public static void Register(String channel, Handler handler)
        {
            Console.WriteLine("Register Signal:" + channel);
            var nc = Engine.Instance.Connection;
            var d = nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
            {
                handler.execute(args.Message.Data);
            });
        }
    }
}