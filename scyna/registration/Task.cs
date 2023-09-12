using Google.Protobuf;

namespace scyna.registration;

public class Task<M> where M : IMessage<M>, new()
{
    readonly string module;
    readonly string channel;
    readonly Registration<M> registration;

    internal Task(Registration<M> registration, string module, string channel)
    {
        this.module = module;
        this.channel = channel;
        this.registration = registration;
    }

    public Request<R> Given<R>() where R : IMessage<R>, new() => new(this);

    public class Request<R> where R : IMessage<R>, new()
    {
        readonly Task<M> Task;
        internal Request(Task<M> Task) => this.Task = Task;

        public Handler<M> When<H>() where H : scyna.Task.Handler<R>, new()
        {
            if (!Registration<M>.testMode) scyna.Task.Register(Task.module, Task.channel, new H());
            return new();
        }
    }
}
