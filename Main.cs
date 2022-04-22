using scyna;

namespace Test
{
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");
            //var engine = Engine.Instance;
        }
    }
}