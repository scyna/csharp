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

    public abstract void Execute();
    public void EventReceived(DomainEventData data)
    {
        this.data = (T)data.Data;
        context.Reset(data.TraceID);
        trace.Reset(data.TraceID);
        try
        {
            Execute();
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
            this.Execute();
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
        Execute();
        trace.Record();
    }
}
