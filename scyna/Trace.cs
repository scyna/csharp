namespace scyna;

public class Trace
{
    public const int SERVICE = 1;
    public const int EVENT = 2;
    public const int SIGNAL = 3;

    private ulong parentID;
    private ulong id;
    private int type;
    private string path;
    private string? source;
    private ulong sessionID;
    private int status;

    private ulong t1;
    public ulong time;

    public ulong ID()
    {
        return id;
    }

    private Trace(string path)
    {
        this.path = path;
    }

    public static Trace newSignalTrace(String channel)
    {
        var ret = new Trace(channel);
        ret.type = SIGNAL;
        ret.sessionID = Engine.SessionID;
        return ret;
    }

    public static Trace newEventTrace(String channel)
    {
        var ret = new Trace(channel);
        ret.type = EVENT;
        ret.sessionID = Engine.SessionID;
        return ret;
    }

    public static Trace newServiceTrace(String url, ulong trace)
    {
        var ret = new Trace(url);
        ret.type = SERVICE;
        ret.id = Engine.ID.Next();
        ret.parentID = trace;
        ret.t1 = 0;//System.nanoTime();
        ret.time = 0;//Utils.currentMicroSeconds();
        ret.source = Engine.Instance.Module;
        return ret;
    }

    public void reset(ulong parent)
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

    public void record()
    {
        // var signal = TraceCreatedSignal.newBuilder()
        //         .setDuration(System.nanoTime() - t1)
        //         .setTime(time)
        //         .setID(id)
        //         .setParentID(parentID)
        //         .setType(type)
        //         .setPath(path)
        //         .setSource(source)
        //         .setSessionID(sessionID)
        //         .setStatus(status).build();
        // SignalLite.emit(Path.TRACE_CREATED_CHANNEL, signal);
    }

}