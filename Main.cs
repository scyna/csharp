using scyna;

namespace Test
{
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("apix","123456");
            var engine = Engine.Instance;
        }
    }
}