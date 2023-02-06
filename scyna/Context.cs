namespace scyna;

using Google.Protobuf;
using scyna.proto;

public class Context : Logger
{
    public Context(ulong id) : base(id, false) { }

    public Response? sendRequest(String url, IMessage request)
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
            var msg1 = Engine.Connection.Request(Utils.PublishURL(url), req.ToByteArray(), 10000);
            var ret = proto.Response.Parser.ParseFrom(msg1.Data);
            trace.Update(ret.SessionID, ret.Code);
            trace.Record();
            return ret;
        }
        catch (Exception) { return null; }
    }

    public long ScheduleTask(String channel, long start, long interval, IMessage data, long loop)
    {

        var task = new scyna.proto.Task
        {
            TraceID = this.ID,
            Data = data.ToByteString(),
        };
        var subject = Engine.Module + "." + channel;

        var response = sendRequest(Path.START_TASK_URL, new StartTaskRequest
        {
            Module = Engine.Module,
            Topic = subject,
            Data = task.ToByteString(),
            Time = start,
            Interval = interval,
            Loop = (ulong)loop,
        });

        if (response == null) throw scyna.Error.REQUEST_INVALID;

        try
        {
            var r = StartTaskResponse.Parser.ParseFrom(response.Body);
            return (long)r.Id;
        }
        catch (InvalidProtocolBufferException e)
        {
            Console.WriteLine(e);
            throw scyna.Error.BAD_DATA;
        }
    }

    public void SaveTag(String key, String value)
    {
        if (this.ID == 0) return;
        Signal.Emit(Path.TRACE_CREATED_CHANNEL, new TagCreatedSignal
        {
            TraceID = this.ID,
            Key = key,
            Value = value,
        });
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

            var subject = Engine.Module + "." + channel;

            Engine.Stream.Publish(subject, ev.ToByteArray());
        }
        catch (IOException) { throw scyna.Error.SERVER_ERROR; }
        catch { throw scyna.Error.STREAM_ERROR; }
    }
}