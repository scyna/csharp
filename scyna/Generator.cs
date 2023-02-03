using Google.Protobuf;
namespace scyna;
public class Generator
{
    private uint prefix;
    private ulong value;
    private ulong end;

    public ulong Next()
    {
        lock (this)
        {
            if (value < end)
            {
                value++;
            }
            else
            {
                if (!getID()) Environment.Exit(1);
            }
            return Utils.CalculateID(prefix, value);
        }
    }

    private bool getID()
    {
        var nc = Engine.Instance.Connection;
        var signal = new proto.Request { TraceID = 0, JSON = false, };
        var msg = nc.Request(Utils.PublishURL(Path.GEN_GET_ID_URL), signal.ToByteArray(), 5000);
        var response = proto.Response.Parser.ParseFrom(msg.Data);
        if (response.Code != 200) return false;
        var id = proto.GetIDResponse.Parser.ParseFrom(response.Body);
        this.prefix = id.Prefix;
        this.value = id.Start;
        this.end = id.End;
        return true;
    }
}
