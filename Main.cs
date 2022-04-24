﻿using scyna;

namespace Test
{
    class HelloService : Service.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive HelloRequest");
            LOG.Info("Test Service log from HelloService");
            Done(new example.HelloResponse { Text = "Hello World" });
        }
    }
    class EchoService : Service.Handler<example.EchoRequest>
    {
        public override void Execute(example.EchoRequest request)
        {
            Console.WriteLine("Receive EchoRequest");
            Done(new example.EchoResponse { Text = request.Text });
        }
    }

    class TestSignal : Signal.Handler<example.TestSignal>
    {
        public override void Execute(example.TestSignal data)
        {
            Console.WriteLine("Receive TestSignal:" + data.Text);
        }
    }

    class EmptySignal : Signal.EmptyHandler
    {
        public override void Execute()
        {
            Console.WriteLine("Receive EmptySignal");
        }
    }

    class TestEvent : Event.Handler<example.TestEvent>
    {
        public override void Execute(example.TestEvent data)
        {
            Console.WriteLine("Receive TestEvent:" + data.Text);
        }
    }

    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Engine.Init("http://127.0.0.1:8081", "scyna.test", "123456");

            Service.Register("/example/echo", new EchoService());
            Service.Register("/example/hello", new HelloService());
            Signal.Register("example.signal.test", new TestSignal());
            Signal.Register("example.signal.empty", new EmptySignal());
            //Event.Register("example.event.test", "consumer", new TestEvent());

            Engine.LOG.Error("Test log form c#");
            Console.WriteLine("Test ID Generator:" + Engine.ID.next());

            var response = Service.SendRequest<example.EchoResponse>("/example/echo", new example.EchoRequest { Text = "Echo" });
            if (response != null) Console.WriteLine("Echo Response:" + response.Text);

            var hello = Service.SendRequest<example.HelloResponse>("/example/hello", null);
            if (hello != null) Console.WriteLine("Hello Response:" + hello.Text);

            Signal.Emit("example.signal.test", new example.TestSignal { Text = "test signal" });
            Signal.Emit("example.signal.empty");

            //Event.Push("example.event.test", new example.TestEvent { Text = "test event" });

            Engine.Start();
            Console.WriteLine("Engine Stopped");
        }
    }
}