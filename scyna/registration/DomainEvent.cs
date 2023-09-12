using Google.Protobuf;

namespace scyna.registration;

public class DomainEvent<M> where M : IMessage<M>, new()
{
    readonly Registration<M> registration;
    internal DomainEvent(Registration<M> registration) => this.registration = registration;
    public Request<R> Given<R>() where R : IMessage<R>, new() => new(this);

    public class Request<R> where R : IMessage<R>, new()
    {
        readonly DomainEvent<M> domainEvent;
        internal Request(DomainEvent<M> domainEvent) => this.domainEvent = domainEvent;

        public Handler<M> When<H>() where H : scyna.DomainEvent.Handler<R>, new()
        {
            if (!Registration<M>.testMode) domainEvent.registration.RegisterDomainEvent<R, H>();
            return new();
        }
    }
}