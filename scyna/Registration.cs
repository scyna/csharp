using Cassandra.Data.Linq;
using Google.Protobuf;

namespace scyna;

public class Registration<M> where M : IMessage<M>, new()
{
    private Registration() { }
    private static readonly Registration<M> instance = new();
    private bool testMode = false;
    public static void TestMode() { instance.testMode = true; }

    public static Registration<M> Start(string table)
    {
        EventStore<M>.New(table);
        return instance;
    }

    public Registration<M> Command<R, H, E, P>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
        where E : IMessage<E>, new()
        where P : Projection<E, M>, new()
    {
        scyna.Endpoint.Register(url, new H());
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> Endpoint<R, H>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
    {
        scyna.Endpoint.Register(url, new H());
        return this;
    }

    public Registration<M> Projection<E, P>()
        where E : IMessage<E>, new()
        where P : Projection<E, M>, new()
    {
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> Command<R, H, E>(string url)
        where R : IMessage<R>, new()
        where H : Endpoint.Handler<R>, new()
        where E : IMessage<E>, new()
    {
        scyna.Endpoint.Register(url, new H());
        return this;
    }

    public Registration<M> Event<E, H, E2, P>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
        where E2 : IMessage<E2>, new()
        where P : Projection<E2, M>, new()
    {
        if (!testMode) scyna.DomainEvent.Register(new H());
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }

    public Registration<M> Event<E, H, E2>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
        where E2 : IMessage<E2>, new()
    {
        if (!testMode) scyna.DomainEvent.Register(new H());
        return this;
    }

    public Registration<M> Event<E, H>()
        where E : IMessage<E>, new()
        where H : DomainEvent.Handler<E>, new()
    {
        if (!testMode) scyna.DomainEvent.Register(new H());
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
}