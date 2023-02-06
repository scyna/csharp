namespace scyna;

using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Threading;
using Google.Protobuf;

public class Task
{
    public static void Register<T>(String sender, String channel, Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Task:" + channel);

        var subject = sender + "." + channel;
        var trace = Trace.NewTaskTrace(subject);
        handler.Init(trace);
        Event.AddToStream(sender, channel, handler);
    }

    public abstract class Handler<T> : Event.IMessageHandler where T : IMessage<T>, new()
    {
        private MessageParser<T> parser = new MessageParser<T>(() => new T());
        protected T data = new T();
        protected Context context = new Context(0);
        protected Trace trace = Trace.NewTaskTrace("");

        public abstract void Execute();
        public void Init(Trace trace) { this.trace = trace; }

        public void MessageReceived(Msg message)
        {
            try
            {
                var task = scyna.proto.Task.Parser.ParseFrom(message.Data);
                this.context.Reset(task.TraceID);
                this.trace.Reset(task.TraceID);
                this.data = parser.ParseFrom(task.Data);
                this.Execute();
                trace.Record();
            }
            catch (InvalidProtocolBufferException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}