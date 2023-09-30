using System.ComponentModel.DataAnnotations;
using Google.Protobuf;

namespace scyna;

public static class Testing
{
    public static EndpointTest Endpoint<T>()
    {
        var t = typeof(T);
        EndpointAttribute attr = (EndpointAttribute)Attribute.GetCustomAttribute(t, typeof(EndpointAttribute));
        if (attr != null) return new(attr.Url);
        else throw new Exception($"Add Enpoint attribute to class {t.Name}");
    }

    public static DomainEventTest<T> DomainEvent<T>(DomainEvent<T> handler)
        where T : IMessage<T>, new() => new(handler);

    public static EventTest<T> Event<T>(Event.Handler<T> handler)
        where T : IMessage<T>, new() => new(handler);

    public static TaskTest<T> Task<T>(Task.Handler<T> handler)
        where T : IMessage<T>, new() => new(handler);
}