namespace ex.account;

using scyna;

public class CreateAccountService : Endpoint.Handler<proto.CreateAccountRequest>
{
    public override void Execute()
    {
        context.Info("Receive CreateAccountRequest");

        var repository = AccountService.loadRepository(context);

        var account = new Account(context);
        account.ID = Engine.ID.Next();
        account.Name = Name.Create(request.Name);
        account.Email = EmailAddress.Parse(request.Email);
        account.Password = Password.Create(request.Password);

        AccountService.AssureAccountNotExists(repository, account.Email);

        var command = new Command
        {
            Entity = account.ID,
            Channel = Path.ACCOUNT_CREATED_CHANNEL,
            Event = new proto.AccountCreated
            {
                Id = account.ID,
                Email = account.Email.ToString(),
                Name = account.Name.ToString(),
            },
        };

        repository.createAccount(command, account);
        command.Commit(context);
        Response(new proto.CreateAccountResponse { Id = account.ID });
    }
}