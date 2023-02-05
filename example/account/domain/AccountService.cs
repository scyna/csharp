namespace ex.account;

using scyna;

public class AccountService
{
    public static IRepository loadRepository(Context context)
    {
        /*TODO inject loader function*/
        return Repository.load(context);
    }

    public static void AssureAccountNotExists(IRepository repository, EmailAddress email)
    {
        try
        {
            repository.GetAccountByEmail(email);
            throw Error.ACCOUNT_EXISTS;
        }
        catch (scyna.Error e)
        {
            if (e != Error.ACCOUNT_NOT_FOUND) throw;
        }
    }

}