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
            /*TODO*/
        }

        public static void Emit(String channel, pb::IMessage message)
        {
            /*TODO*/
        }
        public static void Register(String channel, Handler handler)
        {
            /*TODO*/
        }
    }
}