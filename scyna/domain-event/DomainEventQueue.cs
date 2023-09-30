namespace scyna;

using Google.Protobuf;

public class DomainEventData
{
    public ulong TraceID;
    public IMessage Data;
    public DomainEventData(ulong trace, IMessage data)
    {
        this.TraceID = trace;
        this.Data = data;
    }
}

public interface IDomainEvent { void EventReceived(DomainEventData data); }

public class DomainEventQueue
{
    internal readonly Queue<DomainEventData> queue = new();
    internal readonly Dictionary<Type, List<IDomainEvent>> events = new();
    internal DomainEventQueue() { }

    public static void Clear() => Engine.DomainEventQueue.events.Clear();
    public static void AddEvent(ulong trace, IMessage data) => Engine.DomainEventQueue.queue.Enqueue(new DomainEventData(trace, data));

    public void Start()
    {
        var thread = new Thread(new ThreadStart(Run));
        thread.Start();
    }

    public static IMessage? NextEvent()
    {
        var domainEvent = Engine.DomainEventQueue;
        SpinWait.SpinUntil(() => { return domainEvent.events.Count > 0; }, 1000);
        var item = domainEvent.queue.Dequeue();
        if (item == null) return null;
        return item.Data;
    }

    private void Run()
    {
        while (true)
        {
            SpinWait.SpinUntil(() => { return events.Count > 0; });
            var item = queue.Dequeue();
            var type = item.Data.GetType();
            try
            {
                var handler = events[type];
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
}