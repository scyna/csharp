using Google.Protobuf;

namespace scyna;

public class Model<D> where D : IMessage<D>, new()
{
    public object ID { get; private set; }
    public long Version { get; internal set; }
    private readonly EventStore<D> store;
    public D Data { get; private set; }
    public IMessage Event { get; private set; }

    internal Model(object id, long version, D data, EventStore<D> store)
    {
        this.ID = id;
        this.Version = version;
        this.store = store;
        this.Data = data;
    }

    public void CommitAndProject(IMessage event_)
    {
        this.Event = event_;
        store.UpdateWriteModel(this, event_);
        store.UpdateReadModel(ID);
    }

    public void CommitAndPublish(string channel, IMessage event_)
    {
        this.Event = event_;
        store.UpdateWriteModel(this, event_);
        store.PublishToEventStream(channel, event_);
    }
}