namespace scyna;

using Google.Protobuf;

public class EventData
{
    public string Type { get; internal init; }
    public IMessage Event { get; internal init; }
    public long Version { get; internal init; }
    public DateTimeOffset Time { get; internal init; }

    internal EventData(string type, IMessage event_, long version, DateTimeOffset time)
    {
        this.Type = type;
        this.Event = event_;
        this.Version = version;
        this.Time = time;
    }
}