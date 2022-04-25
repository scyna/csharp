using NUnit.Framework;
using Google.Protobuf;

namespace scyna
{
    class Test
    {
        public static void TestService<T>(string url, IMessage request, T response, uint code) where T : IMessage<T>, new()
        {
            var res = Service.SendRequest(url, request);
            Assert.IsNotNull(res);
            Assert.Equals(res.Code, code);

            try
            {
                MessageParser<T> parser = new MessageParser<T>(() => new T());
                var r = parser.ParseFrom(res.Body);
                Assert.True(response.Equals(r));
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}