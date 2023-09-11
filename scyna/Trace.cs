namespace scyna;

public class Trace
{
    public const int ENDPOINT = 1;
    public const int EVENT = 2;
    public const int SYNC = 4;
    public const int TASK = 5;
    public const int DOMAIN_EVENT = 6;

    private ulong parentID;
    private ulong id;
    private uint type;
    private readonly string path;
    private ulong sessionID;

    private ulong t1;
    public ulong time;

    public ulong ID() { return id; }

    private Trace(string path)
    {
        this.path = path;
    }

    public static Trace NewEventTrace(String channel)
    {
        var ret = new Trace(channel)
        {
            type = EVENT,
            sessionID = Engine.SessionID,
        };
        return ret;
    }

    public static Trace NewDomainEventTrace(String name)
    {
        var ret = new Trace(name)
        {
            type = DOMAIN_EVENT,
            sessionID = Engine.SessionID,
        };
        return ret;
    }


    public static Trace NewTaskTrace(String channel)
    {
        var ret = new Trace(channel)
        {
            type = TASK,
            sessionID = Engine.SessionID,
        };
        return ret;
    }


    public static Trace NewEndpointTrace(String url, ulong trace)
    {
        var ret = new Trace(url)
        {
            type = ENDPOINT,
            id = Engine.ID.Next(),
            parentID = trace,
            t1 = (ulong)Utils.GetNanoseconds(),
            time = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
        };
        return ret;
    }

    public void Reset(ulong parent)
    {
        this.parentID = parent;
        this.id = Engine.ID.Next();
        t1 = (ulong)Utils.GetNanoseconds();
        time = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    // public void Update(ulong session, int status)
    // {
    //     this.sessionID = session;
    //     this.status = status;
    // }

    // public void start()
    // {
    //     t1 = (ulong)Utils.GetMicroseconds();
    //     time = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    // }

    public void Record()
    {
        var signal = new proto.TraceCreatedSignal
        {
            Duration = (ulong)Utils.GetNanoseconds() - t1,
            Time = time,
            ID = id,
            ParentID = parentID,
            Type = type,
            Path = path,
            SessionID = sessionID,
        };
        Signal.Emit(Path.TRACE_CREATED_CHANNEL, signal);
    }
}