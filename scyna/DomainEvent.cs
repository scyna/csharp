namespace scyna;

using Google.Protobuf;

public class DomainEvent
{
    static DomainEvent domainEvent = new DomainEvent();
    Queue<EventData> events = new Queue<EventData>();
    Dictionary<Type, List<IHandler>> handlers = new Dictionary<Type, List<IHandler>>();

    private DomainEvent() { }

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
        protected T data = new T();
        protected Context context = new Context(0);
        protected Trace trace = Trace.NewDomainEventTrace();

        public abstract void Execute();
        public void EventReceived(EventData data)
        {
            this.data = (T)data.Data;
            context.Reset(data.TraceID);
            trace.Reset(data.TraceID);
            Execute();
            trace.Record();
        }
    }

    private void run()
    {
        while (true)
        {
            SpinWait.SpinUntil(() => { return events.Count > 0; });
            var item = events.Dequeue();
            var type = item.Data.GetType();
            var handler = handlers[type];
            if (handler != null)
            {
                foreach (var h in handler)
                {
                    h.EventReceived(item);
                }
            }
        }
    }

    public static void Register<T>(Handler<T> handler) where T : IMessage<T>, new()
    {
        var list = domainEvent.handlers[typeof(T)];
        if (list == null)
        {
            list = new List<IHandler>();
            domainEvent.handlers[typeof(T)] = list;
        }
        list.Add(handler);
    }

    public static void AddEvent(ulong trace, IMessage data)
    {
        domainEvent.events.Enqueue(new EventData(trace, data));
    }

    public static void Start()
    {
        var thread = new Thread(new ThreadStart(domainEvent.run));
        thread.Start();
    }

    public static IMessage? NextEvent()
    {
        SpinWait.SpinUntil(() => { return domainEvent.events.Count > 0; }, 1000);
        var item = domainEvent.events.Dequeue();
        if (item == null) return null;
        return item.Data;
    }
}
