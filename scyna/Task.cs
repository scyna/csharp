namespace scyna;

using System;
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
        private readonly MessageParser<T> parser = new(() => new T());
        protected T data = new();
        protected Context context = new(0);
        protected Trace trace = Trace.NewTaskTrace("");

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
                var task = scyna.proto.Task.Parser.ParseFrom(message);
                this.context.Reset(task.TraceID);
                this.trace.Reset(task.TraceID);
                this.data = parser.ParseFrom(task.Data);
                this.Execute();
                trace.Record();
            }
            catch (Exception e) { OnError(e); }
        }
    }
}