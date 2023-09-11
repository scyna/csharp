namespace scyna;

using Google.Protobuf;
using scyna.proto;

public class Context : Logger
{
    public Context(ulong id) : base(id) { }

    public void Command(String url, IMessage request, scyna.Error error)
    {
        try { SendRequest(url, request); }
        catch { throw error; }
    }

    public T Query<T>(String url, IMessage request, scyna.Error error) where T : IMessage<T>, new()
    {
        try { return SendRequest<T>(url, request); }
        catch { throw error; }
    }

    public void SendRequest(String url, IMessage request)
    {
        Trace trace = Trace.NewEndpointTrace(url, this.ID);
        var req = new scyna.proto.Request
        {
            TraceID = trace.ID(),
            JSON = false,
        };
        if (request != null) req.Body = request.ToByteString();
        try
        {
            var msg = Engine.Connection.Request(Utils.PublishURL(url), req.ToByteArray(), 10000);
            var ret = proto.Response.Parser.ParseFrom(msg.Data);
            trace.Record();
            if (ret.Code != 200)
            {
                var err = scyna.proto.Error.Parser.ParseFrom(ret.Body);
                throw new scyna.Error(err.Code, err.Message);
            }
        }
        catch (scyna.Error) { throw; }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            throw scyna.Error.API_CALL_ERROR;
        }
    }

    public T SendRequest<T>(String url, IMessage request) where T : IMessage<T>, new()
    {
        Trace trace = Trace.NewEndpointTrace(url, this.ID);
        var req = new scyna.proto.Request
        {
            TraceID = trace.ID(),
            JSON = false,
        };
        if (request != null) req.Body = request.ToByteString();
        try
        {
            var msg = Engine.Connection.Request(Utils.PublishURL(url), req.ToByteArray(), 10000);
            var ret = proto.Response.Parser.ParseFrom(msg.Data);
            trace.Record();
            if (ret.Code != 200)
            {
                var err = scyna.proto.Error.Parser.ParseFrom(ret.Body);
                throw new scyna.Error(err.Code, err.Message);
            }
            MessageParser<T> parser = new(() => new T());
            return parser.ParseFrom(ret.Body);
        }
        catch (scyna.Error) { throw; }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            throw scyna.Error.API_CALL_ERROR;
        }
    }

    public long ScheduleOne(String channel, DateTimeOffset time, IMessage data)
    {
        return ScheduleTask(channel, time.ToUnixTimeSeconds(), 60, data, 1);
    }

    public long ScheduleOne(String channel, long duration, IMessage data)
    {
        return ScheduleTask(channel, duration, 60, data, 1);
    }

    public long ScheduleTask(String channel, long start, long interval, IMessage data, long loop)
    {
        var task = new scyna.proto.Task
        {
            TraceID = this.ID,
            Data = data.ToByteString(),
        };

        var r = SendRequest<StartTaskResponse>(Path.START_TASK_URL, new StartTaskRequest
        {
            Module = Engine.Module,
            Topic = $"{Engine.Module}.{channel}",
            Data = task.ToByteString(),
            Time = start,
            Interval = interval,
            Loop = (ulong)loop,
        });

        return (long)r.Id;
    }

    public void PublishEvent(String channel, IMessage data)
    {
        try
        {
            var ev = new scyna.proto.Event
            {
                TraceID = this.ID,
                Body = data.ToByteString()
            };

            Engine.Stream.Publish($"{Engine.Module}.{channel}", ev.ToByteArray());
        }
        catch (IOException) { throw scyna.Error.SERVER_ERROR; }
        catch { throw scyna.Error.STREAM_ERROR; }
    }

    public void RaiseDomainEvent(IMessage data)
    {
        DomainEvent.AddEvent(this.ID, data);
    }
}