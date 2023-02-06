namespace ex.account;

using scyna;

public class SendEmailHandler : Task.Handler<proto.SendEmailTask>
{
    public override void Execute()
    {
        context.Info("Receive SendEmailTask");
        Console.WriteLine(data);
    }
}