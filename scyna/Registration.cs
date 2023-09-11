using Google.Protobuf;

namespace scyna;

public class Registration<M> where M : IMessage<M>, new()
{
    private Registration() { }
    private static readonly Registration<M> instance = new();
    private bool testMode = false;
    public static void TestMode() { instance.testMode = true; }

    private Dictionary<string, Type> endpoints = new();
    private List<Type> domainEvents = new();

    public static Registration<M> Start(string table)
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

    public Registration<M> Command<R, H, E, P>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
        where E : IMessage<E>, new()
        where P : Projection<E, M>, new()
    {
        RegisterEndpoint<R, H>(url);
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> Endpoint<R, H>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
    {
        RegisterEndpoint<R, H>(url);
        return this;
    }

    public Registration<M> Command<R, H, E>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
        where E : IMessage<E>, new()
    {
        RegisterEndpoint<R, H>(url);
        return this;
    }

    public Registration<M> DomainEvent<E, H, E2, P>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
        where E2 : IMessage<E2>, new()
        where P : Projection<E2, M>, new()
    {
        if (!testMode) RegisterDomainEvent<E, H>();
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> DomainEvent<E, H>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
    {
        if (!testMode) RegisterDomainEvent<E, H>();
        return this;
    }

    public Registration<M> Event<E, H, E2, P>(string module, string channel)
        where E : IMessage<E>, new()
        where H : Event.Handler<E>, new()
        where E2 : IMessage<E2>, new()
        where P : Projection<E2, M>, new()
    {
        if (!testMode) scyna.Event.Register(module, channel, new H());
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> Event<E, H>(string module, string channel)
    where E : IMessage<E>, new()
    where H : Event.Handler<E>, new()
    {
        if (!testMode) scyna.Event.Register(module, channel, new H());
        return this;
    }

    private void RegisterEndpoint<R, H>(string url)
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

    private void RegisterDomainEvent<E, H>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
    {
        var t = domainEvents.Find(x => x == typeof(H));
        if (t == null)
        {
            domainEvents.Add(typeof(H));
            scyna.DomainEvent.Register(new H());
        }
    }
}