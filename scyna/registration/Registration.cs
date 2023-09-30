using Google.Protobuf;

namespace scyna;

public class Registration<M> where M : IMessage<M>, new()
{
    private Registration() { }
    private static readonly Registration<M> instance = new();
    internal static bool testMode = false;
    public static void TestMode() { testMode = true; }

    private Dictionary<string, Type> endpoints = new();
    private List<Type> domainEvents = new();

    public static Registration<M> Create(string table)
    {
        EventStore<M>.New(table);
        return instance;
    }

    public void Reset()
    {
        endpoints = new();
        domainEvents = new();
        EventStore<M>.Reset();
    }

    public registration.Command<M> Command(string url) => new(this, url);
    public registration.DomainEvent<M> DomainEvent() => new(this);
    public registration.Event<M> Event(string module, string channel) => new(module, channel);
    public registration.Task<M> Task(string module, string channel) => new(module, channel);
    public void Endpoint<R, H>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
        => RegisterEndpoint<R, H>(url);

    internal void RegisterEndpoint<R, H>(string url)
    where R : IMessage<R>, new()
    where H : Endpoint.Handler<R>, new()
    {
        if (endpoints.ContainsKey(url))
        {
            var t = endpoints[url];
            if (t != typeof(H)) throw new Exception($"Duplicate endpoint {url}");
        }
        else
        {
            endpoints.Add(url, typeof(H));
            scyna.Endpoint.Register(url, new H());
        }
    }

    internal void RegisterDomainEvent<E, H>()
        where E : IMessage<E>, new()
        where H : scyna.DomainEvent.Handler<E>, new()
    {
        var t = domainEvents.Find(x => x == typeof(H));
        if (t == null)
        {
            if (testMode) return;
            domainEvents.Add(typeof(H));
            scyna.DomainEvent.Register(new H());
        }
    }
}