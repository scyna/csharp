namespace scyna;

public class SerialNumber
{
    private string key;
    private uint prefix = 0;
    private ulong last = 0;
    private ulong next = 0;

    public SerialNumber(string key) { this.key = key; }

    public string Next()
    {
        lock (this)
        {
            if (next < last) next++;
            else
            {
                var response = Endpoint.SendRequest<proto.GetSNResponse>(
                    Path.GEN_GET_SN_URL,
                    new proto.GetSNRequest { Key = key }
                );
                if (response != null)
                {
                    prefix = response.Prefix;
                    next = response.Start;
                    last = response.End;
                }
            }
            return String.Format("%d%07d", prefix, next);
        }
    }
}
