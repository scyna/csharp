namespace scyna;

using Google.Protobuf;

public abstract class DomainEvent<T> : IDomainEvent where T : IMessage<T>, new()
{
    protected T data = new();
    protected Context context = new(0);
    protected Trace trace = Trace.NewDomainEventTrace(typeof(T).Name);

    protected virtual void OnError(Exception e)
    {
        context.Error(e.ToString());
    }

    public abstract void Handle();
    public void EventReceived(DomainEventData data)
    {
        this.data = (T)data.Data;
        context.Reset(data.TraceID);
        trace.Reset(data.TraceID);
        try
        {
            Handle();
        }
        catch (scyna.Error e)
        {
            if (e == Error.COMMAND_NOT_COMPLETED)
            {
                for (int i = 0; i < 5; i++) { if (Retry()) return; }
            }
            OnError(e);
        }
        catch (Exception e)
        {
            OnError(e);
        }
        trace.Record();
    }

    private bool Retry()
    {
        try
        {
            this.Handle();
        }
        catch (scyna.Error e)
        {
            if (e == Error.COMMAND_NOT_COMPLETED) return false;
            OnError(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            OnError(e);
        }
        return true;
    }

    internal void TestEventReceived(DomainEventData data)
    {
        this.data = (T)data.Data;
        context.Reset(data.TraceID);
        trace.Reset(data.TraceID);
        Handle();
        trace.Record();
    }
}
