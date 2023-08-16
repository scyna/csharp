namespace scyna;

public class SerialNumber
{
    public class Value
    {
        private uint prefix;
        private ulong value;
        internal Value(uint prefix, ulong value)
        {
            this.prefix = prefix;
            this.value = value;
        }

        public string GetTwelveDigitsString()
        {
            return String.Format("{0:00000}{1:0000000}", prefix, value);
        }

        public string GetTenDigitsString()
        {
            return String.Format("{0:00000}{1:00000}", prefix, value);
        }

        public ulong GetNumber()
        {
            return Utils.CalculateID(prefix, value);
        }
    }

    private string key;
    private uint prefix = 0;
    private ulong last = 0;
    private ulong next = 0;

    public SerialNumber(string key) { this.key = key; }

    public Value Next()
    {
        lock (this)
        {
            if (next < last) next++;
            else
            {
                var response = Request.Send<proto.GetSNResponse>(
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
            return new Value(prefix, next);
        }
    }
}
