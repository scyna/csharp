namespace scyna;
using Google.Protobuf;
using Xunit;

public class TaskTest<T> : BaseTest<TaskTest<T>>
    where T : IMessage<T>, new()
{
    private readonly Task.Handler<T> handler;
    private IMessage? input;
    private bool expectSuccess = true;

    internal TaskTest(Task.Handler<T> handler) { this.handler = handler; }

    public TaskTest<T> WithData(IMessage data)
    {
        this.input = data;
        return this;
    }

    public TaskTest<T> ShouldBeFine()
    {
        expectSuccess = true;
        return Run();
    }

    public TaskTest<T> ShouldFail()
    {
        expectSuccess = false;
        return Run();
    }

    private TaskTest<T> Run()
    {
        DomainEvent.Clear();
        try
        {
            if (input is null) throw new Exception("Input is null");

            CreateStream();
            var trace = Trace.NewTaskTrace("");
            handler.Init(trace);
            var task = new proto.Task
            {
                TraceID = (ulong)Engine.ID.Next(),
                Data = input.ToByteString(),
            };

            handler.MessageReceived(task.ToByteArray());
            ReceiveDomainEvent();
            ReceiveEvent();
            DeleteStream();

            if (expectSuccess) return this;
            expectSuccess = true;
            Assert.False(expectSuccess);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Assert.False(expectSuccess);
        }
        return this;
    }
}
