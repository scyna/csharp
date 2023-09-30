namespace scyna;

using NATS.Client.JetStream;
using System;
using System.Threading;
using Google.Protobuf;

public class Event
{
    static readonly Dictionary<string, Stream> streams = new();

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
        readonly string sender;
        readonly IJetStreamPullSubscription subscription;
        public Dictionary<string, IMessageHandler> executors;

        Stream(String sender, String receiver)
        {
            this.sender = sender;
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
            var thread = new Thread(new ThreadStart(this.Run));
            thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                var messages = subscription.Fetch(1, 1000); //1000ms
                foreach (NATS.Client.Msg m in messages)
                {
                    try
                    {
                        var executor = executors[m.Subject];
                        executor?.MessageReceived(m.Data);
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
        private readonly MessageParser<T> parser = new(() => new T());
        protected T data = new();
        protected Context context = new(0);
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
                this.data = parser.ParseFrom(ev.Body);
                this.Execute();
                trace.Record();
            }
            catch (scyna.Error e)
            {
                if (e == Error.COMMAND_NOT_COMPLETED)
                {
                    for (int i = 0; i < 5; i++) { if (Retry()) return; }
                }
                OnError(e);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private bool Retry()
        {
            try
            {
                this.Execute();
            }
            catch (scyna.Error e)
            {
                if (e == Error.COMMAND_NOT_COMPLETED) return false;
                OnError(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OnError(e);
            }
            return true;
        }
    }
}
