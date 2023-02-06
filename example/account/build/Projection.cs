namespace ex.account;

using scyna;

public class Projecttion
{
    static void Main(string[] args)
    {
        Engine.Init("http://127.0.0.1:8081", "ex_account", "123456");
        Event.Register("ex_account", Path.ACCOUNT_CREATED_CHANNEL, new AccountCreatedHandler());
        Task.Register("ex_account", Path.SEND_EMAIL_CHANNEL, new SendEmailHandler());
        Event.StartListening();
        Engine.Start();
    }
}