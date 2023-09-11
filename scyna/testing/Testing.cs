using Google.Protobuf;

namespace scyna;

public static class Testing
{
    public static EndpointTest Endpoint(string url) => new(url);

    public static DomainEventTest<T> DomainEvent<T>(DomainEvent.Handler<T> handler)
        where T : IMessage<T>, new() => new(handler);

    public static EventTest<T> Event<T>(Event.Handler<T> handler)
        where T : IMessage<T>, new() => new(handler);

    public static TaskTest<T> Task<T>(Task.Handler<T> handler)
        where T : IMessage<T>, new() => new(handler);
}