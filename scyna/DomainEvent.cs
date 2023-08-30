namespace scyna;

using Google.Protobuf;

public class DomainEvent
{
    readonly Queue<EventData> events = new();
    readonly Dictionary<Type, List<IHandler>> handlers = new();

    public interface IHandler
    {
        void EventReceived(EventData data);
    }
    public class EventData
    {
        public ulong TraceID;
        public IMessage Data;
        public EventData(ulong trace, IMessage data)
        {
            this.TraceID = trace;
            this.Data = data;
        }
    }

    public abstract class Handler<T> : IHandler where T : IMessage<T>, new()
    {
        protected T data = new();
        protected Context context = new(0);
        protected Trace trace = Trace.NewDomainEventTrace(typeof(T).Name);

        protected virtual void OnError(Exception e)
        {
            context.Error(e.ToString());
        }

        public abstract void Execute();
        public void EventReceived(EventData data)
        {
            this.data = (T)data.Data;
            context.Reset(data.TraceID);
            trace.Reset(data.TraceID);
            try
            {
                Execute();
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
            trace.Record();
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

        internal void TestEventReceived(EventData data)
        {
            this.data = (T)data.Data;
            context.Reset(data.TraceID);
            trace.Reset(data.TraceID);
            Execute();
            trace.Record();
        }
    }

    private void Run()
    {
        while (true)
        {
            SpinWait.SpinUntil(() => { return events.Count > 0; });
            var item = events.Dequeue();
            var type = item.Data.GetType();
            try
            {
                var handler = handlers[type];
                if (handler != null)
                {
                    foreach (var h in handler)
                    {
                        h.EventReceived(item);
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Event not registered: " + type.Name);
            }
        }
    }

    public static void Register<T>(Handler<T> handler) where T : IMessage<T>, new()
    {
        Console.WriteLine("Register Domain Event: " + typeof(T).Name + " " + handler.GetType().Name);
        var domainEvent = Engine.DomainEvent;
        try
        {
            var list = domainEvent.handlers[typeof(T)];
            list.Add(handler);
        }
        catch (KeyNotFoundException)
        {
            var list = new List<IHandler>();
            domainEvent.handlers[typeof(T)] = list;
            list.Add(handler);
        }
    }

    public static void Clear()
    {
        Engine.DomainEvent.events.Clear();
    }

    public static void AddEvent(ulong trace, IMessage data)
    {
        Engine.DomainEvent.events.Enqueue(new EventData(trace, data));
    }

    public static void Start()
    {
        var thread = new Thread(new ThreadStart(Engine.DomainEvent.Run));
        thread.Start();
    }

    public static IMessage? NextEvent()
    {
        var domainEvent = Engine.DomainEvent;
        SpinWait.SpinUntil(() => { return domainEvent.events.Count > 0; }, 1000);
        var item = domainEvent.events.Dequeue();
        if (item == null) return null;
        return item.Data;
    }
}
