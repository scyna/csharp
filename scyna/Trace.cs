namespace scyna;

public class Trace
{
    public const int ENDPOINT = 1;
    public const int EVENT = 2;
    public const int SYNC = 4;
    public const int TASK = 5;

    private ulong parentID;
    private ulong id;
    private uint type;
    private string path;
    private string? source;
    private ulong sessionID;
    private int status;

    private ulong t1;
    public ulong time;

    public ulong ID() { return id; }

    private Trace(string path)
    {
        this.path = path;
    }

    public static Trace NewEventTrace(String channel)
    {
        var ret = new Trace(channel);
        ret.type = EVENT;
        ret.sessionID = Engine.SessionID;
        ret.source = Engine.Module;
        return ret;
    }

    public static Trace NewTaskTrace(String channel)
    {
        var ret = new Trace(channel);
        ret.type = TASK;
        ret.sessionID = Engine.SessionID;
        ret.source = Engine.Module;
        return ret;
    }


    public static Trace NewEndpointTrace(String url, ulong trace)
    {
        var ret = new Trace(url);
        ret.type = ENDPOINT;
        ret.id = Engine.ID.Next();
        ret.parentID = trace;
        ret.t1 = 0;//System.nanoTime();
        ret.time = 0;//Utils.currentMicroSeconds();
        ret.source = Engine.Module;
        return ret;
    }

    public void Reset(ulong parent)
    {
        this.parentID = parent;
        this.id = Engine.ID.Next();
        t1 = 0;//System.nanoTime();
        time = 0;//Utils.currentMicroSeconds();
    }

    public void update(ulong session, int status)
    {
        this.sessionID = session;
        this.status = status;
    }

    public void start()
    {
        t1 = 0;//System.nanoTime();
        time = 0;//Utils.currentMicroSeconds();
    }

    public void Record()
    {
        var signal = new proto.TraceCreatedSignal
        {
            Duration = 0, //FIXME
            Time = 0, //FIXME
            ID = id,
            ParentID = parentID,
            Type = type,
            Path = path,
            Source = source,
            SessionID = sessionID,
            Status = status,
        };
        Signal.Emit(Path.TRACE_CREATED_CHANNEL, signal);
    }
}