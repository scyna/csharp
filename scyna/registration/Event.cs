using Google.Protobuf;

namespace scyna.registration;

public class Event<M> where M : IMessage<M>, new()
{
    readonly string module;
    readonly string channel;

    internal Event(string module, string channel)
    {
        this.module = module;
        this.channel = channel;
    }

    public Request<R> Given<R>() where R : IMessage<R>, new() => new(this);

    public class Request<R> where R : IMessage<R>, new()
    {
        readonly Event<M> Event;
        internal Request(Event<M> Event) => this.Event = Event;

        public Handler<M> When<H>() where H : scyna.Event.Handler<R>, new()
        {
            if (!Registration<M>.testMode) scyna.Event.Register(Event.module, Event.channel, new H());
            return new();
        }
    }
}
