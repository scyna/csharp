namespace scyna;
using Google.Protobuf;

internal interface IProjection
{
    void Update(byte[] event_, byte[] data);
    bool Matched(string type);
    string Type();
    IMessage? ParseEvent(byte[] data);
}

public abstract class Projection<E, D> : IProjection
    where D : IMessage<D>, new()
    where E : IMessage<E>, new()
{
    protected abstract void Execute(E event_, D data);
    public bool Matched(string type) { return type == typeof(E).Name; }
    public string Type() { return typeof(E).Name; }

    private static readonly MessageParser<D> dataParser = new(() => new D());
    private static readonly MessageParser<E> eventParser = new(() => new E());

    void IProjection.Update(byte[] event_, byte[] data)
    {
        try
        {
            var dataObject = dataParser.ParseFrom(data);
            var eventObject = eventParser.ParseFrom(event_);
            Execute(eventObject, dataObject);
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
        }
    }

    public IMessage? ParseEvent(byte[] data)
    {
        try
        {
            return eventParser.ParseFrom(data);
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
        }
        return default(E);
    }
}