using Google.Protobuf;

namespace scyna.registration;

public class Command<M> where M : IMessage<M>, new()
{
    readonly string url;
    readonly Registration<M> registration;
    internal Command(Registration<M> registration, string url)
    {
        this.url = url;
        this.registration = registration;
    }
    public Request<R> Given<R>() where R : IMessage<R>, new() => new(this);

    public class Request<R> where R : IMessage<R>, new()
    {
        readonly Command<M> command;
        internal Request(Command<M> command) => this.command = command;
        public Handler<M> When<H>() where H : Endpoint.Handler<R>, new()
        {
            command.registration.RegisterEndpoint<R, H>(command.url);
            return new();
        }
    }
}