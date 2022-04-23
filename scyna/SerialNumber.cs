namespace scyna
{
    class SerialNumber
    {
        private  string key;
        private uint prefix = 0;
        private ulong last = 0;
        private ulong next = 0;

        public SerialNumber(string key) {
            this.key = key;
        }

        public string Next()
        {
            lock(this)
            {
            if (next < last) {
                next++;
            } else {
                var request = new proto.GetSNRequest{Key = key};
                
                // var response = Service.sendRequest(Path.GEN_GET_SN_URL, request);
                // if (response != null && response.getCode() == 200) {
                //     var r = GetSNResponse.parseFrom(response.getBody());
                //     prefix = r.getPrefix();
                //     next = r.getStart();
                //     last = r.getEnd();
                // }
            }
            return String.Format("%d%07d", prefix, next);
            }
        }
        
    }
}