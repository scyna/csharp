namespace scyna;

using NATS.Client;
using NATS.Client.JetStream;

public class Event
{
    static Dictionary<string, Stream> streams = new Dictionary<string, Stream>();

    interface IMessageHandler
    {
        void onMessage(NATS.Client.Msg message);
    }

    public static void StartListening()
    {
        foreach (var stream in streams.Values) { stream.Start(); }
    }

    class Stream
    {
        string sender;
        string receiver;
        Dictionary<string, IMessageHandler> executors;

        Stream(String sender, String receiver)
        {
            this.sender = sender;
            this.receiver = receiver;
            executors = new Dictionary<string, IMessageHandler>();
        }

        private static Stream CreateOrGet(String sender)
        {
            var stream = streams[sender];
            if (stream != null) return stream;

            stream = new Stream(sender, Engine.Module);
            streams[sender] = stream;
            return stream;
        }

        public void Start()
        {

        }
    }


}
