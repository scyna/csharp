namespace ex.account;

using scyna;

public class AccountCreatedHandler : Event.Handler<proto.AccountCreated>
{
    public override void Execute()
    {
        context.Info("Receive AccountCreated event");
        Console.WriteLine(data);
        try
        {
            context.ScheduleTask(Path.SEND_EMAIL_CHANNEL, DateTimeOffset.Now.ToUnixTimeSeconds(), 61,
                    new proto.SendEmailTask { Email = data.Email, Content = "Just say hello" }, 2);
        }
        catch (scyna.Error e) { Console.WriteLine(e); }
    }
}