using Google.Protobuf;
using pb = global::Google.Protobuf;

namespace scyna;

public class Signal
{
    public static void Emit(string channel)
    {
        var msg = new proto.EventOrSignal { CallID = Engine.ID.Next() };
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, msg.ToByteArray());
    }
    public static void Emit(string channel, pb::IMessage message)
    {
        var msg = new proto.EventOrSignal { CallID = Engine.ID.Next(), Body = message.ToByteString() };
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, msg.ToByteArray());
    }
    public static void Register<T>(string channel, StatefulHandler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Signal:" + channel);
        var nc = Engine.Instance.Connection;
        MessageParser<T> parser = new MessageParser<T>(() => new T());
        nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
        {
            try
            {
                var msg = proto.EventOrSignal.Parser.ParseFrom(args.Message.Data);
                handler.ResetLog(msg.CallID);
                handler.Run(msg.Body.ToByteArray());
            }
            catch (Exception) { }
        });
    }

    public static void Register(string channel, StatelessHandler handler)
    {
        Console.WriteLine("Register Signal:" + channel);
        var nc = Engine.Instance.Connection;
        nc.SubscribeAsync(channel, Engine.Instance.Module, (sender, args) =>
        {
            try
            {
                var msg = proto.EventOrSignal.Parser.ParseFrom(args.Message.Data);
                handler.ResetLog(msg.CallID);
                handler.Execute();
            }
            catch (Exception) { }
        });
    }

    public abstract class StatefulHandler<T> where T : IMessage<T>, new()
    {
        protected Logger LOG = new Logger(0, false);
        private MessageParser<T> parser = new MessageParser<T>(() => new T());
        public void ResetLog(ulong id) { this.LOG.Reset(id); }
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

    public abstract class StatelessHandler
    {
        protected Logger LOG = new Logger(0, false);
        public void ResetLog(ulong id) { this.LOG.Reset(id); }
        public abstract void Execute();
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
    public static void Send(string channel, pb::IMessage message)
    {
        var nc = Engine.Instance.Connection;
        nc.Publish(channel, message.ToByteArray());
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
