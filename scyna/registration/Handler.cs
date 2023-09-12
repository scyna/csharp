using Google.Protobuf;

namespace scyna.registration;

public class Handler<M>
    where M : IMessage<M>, new()
{
    public Handler<M> Then<E>() where E : IMessage<E>, new() => this;
    public Handler<M> Then<E, P>()
        where E : IMessage<E>, new()
        where P : Projection<E, M>, new()
    {
        EventStore<M>.Instance().RegisterProjection(new P());
        return this;
    }
}
