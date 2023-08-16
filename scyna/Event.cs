namespace scyna;

using NATS.Client.JetStream;
using System;
using System.Threading;
using Google.Protobuf;

public class Event
{
    static Dictionary<string, Stream> streams = new Dictionary<string, Stream>();

    public interface IMessageHandler
    {
        void MessageReceived(byte[] message);
    }

    public static void Start()
    {
        Console.WriteLine("Event loop started");
        var tmp = streams.Values;
        Console.WriteLine($"{tmp.Count}");
        foreach (var stream in streams.Values) { stream.Start(); }
    }

    public static void Register<T>(String sender, String channel, Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Event:" + channel);

        var stream = Stream.CreateOrGet(sender);
        var subject = $"{sender}.{channel}";
        var trace = Trace.NewEventTrace(subject);
        handler.Init(trace);
        stream.executors[subject] = handler;
    }

    public static void AddToStream(String sender, String channel, IMessageHandler handler)
    {
        var stream = Stream.CreateOrGet(sender);
        stream.executors[$"{sender}.{channel}"] = handler;
    }

    class Stream
    {
        string sender;
        string receiver;
        IJetStreamPullSubscription subscription;
        public Dictionary<string, IMessageHandler> executors;

        Stream(String sender, String receiver)
        {
            this.sender = sender;
            this.receiver = receiver;
            executors = new Dictionary<string, IMessageHandler>();
            var options = PullSubscribeOptions.Builder().WithDurable(receiver).WithStream(sender).Build();
            subscription = Engine.Stream.PullSubscribe($"{sender}.>", options);
        }

        public static Stream CreateOrGet(String sender)
        {
            if (streams.ContainsKey(sender)) return streams[sender];
            var stream = new Stream(sender, Engine.Module);
            streams[sender] = stream;
            return stream;
        }

        public void Start()
        {
            Console.WriteLine($"Stream started {sender}");
            var thread = new Thread(new ThreadStart(this.run));
            thread.Start();
        }

        private void run()
        {
            while (true)
            {
                var messages = subscription.Fetch(1, 1000); //1000ms
                foreach (NATS.Client.Msg m in messages)
                {
                    try
                    {
                        var executor = executors[m.Subject];
                        if (executor != null) executor.MessageReceived(m.Data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    m.Ack();
                }
            }
        }
    }

    public abstract class Handler<T> : IMessageHandler where T : IMessage<T>, new()
    {
        private MessageParser<T> parser = new MessageParser<T>(() => new T());
        protected T data = new T();
        protected Context context = new Context(0);
        protected Trace trace = Trace.NewEventTrace("");
        protected ulong entity;
        protected ulong version;

        public abstract void Execute();
        public void Init(Trace trace) { this.trace = trace; }

        protected virtual void OnError(Exception e)
        {
            context.Error(e.ToString());
        }

        public void MessageReceived(byte[] message)
        {
            try
            {
                var ev = scyna.proto.Event.Parser.ParseFrom(message);
                this.context.Reset(ev.TraceID);
                this.trace.Reset(ev.TraceID);
                this.entity = ev.Entity;
                this.version = ev.Version;
                this.data = parser.ParseFrom(ev.Body);
                this.Execute();
                trace.Record();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
