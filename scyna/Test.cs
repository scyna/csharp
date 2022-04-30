namespace scyna;
using NUnit.Framework;
using Google.Protobuf;

public class Test
{
    public static void TestService(string url, IMessage request, uint code)
    {
        var res = Service.SendRequest(url, request);
        Assert.IsNotNull(res);
        Assert.AreEqual(code, res.Code);
    }

    public static void TestService<T>(string url, IMessage request, T response, uint code) where T : IMessage<T>, new()
    {
        var res = Service.SendRequest(url, request);
        Assert.IsNotNull(res);
        Assert.AreEqual(code, res.Code);

        try
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            var r = parser.ParseFrom(res.Body);
            Assert.IsTrue(response.Equals(r));
        }
        catch
        {
            Assert.Fail();
        }
    }

    public static T CallService<T>(string url, IMessage request) where T : IMessage<T>, new()
    {
        var res = Service.SendRequest(url, request);
        Assert.IsNotNull(res);
        Assert.AreEqual(200, res.Code);

        try
        {
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(res.Body);
        }
        catch
        {
            Assert.Fail();
            return default(T);
        }
    }
}
