using Google.Protobuf;

namespace scyna.registration;

public class Task<M> where M : IMessage<M>, new()
{
    readonly string module;
    readonly string channel;

    internal Task(string module, string channel)
    {
        this.module = module;
        this.channel = channel;
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
