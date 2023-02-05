namespace ex.account;

using scyna;
using Scylla.Net;

public class Repository : IRepository
{
    const string TABLE_NAME = "account";
    const string KEYSPACE = "ex_account";
    private Context context;

    private Repository(Context context) { this.context = context; }

    public static IRepository load(Context context)
    {
        return new Repository(context);
    }

    public void createAccount(Command command, Account account)
    {

    }

    public Account GetAccountByEmail(EmailAddress email)
    {
        var query = Engine.DB.Session.Prepare(string.Format("SELECT id,email,name FROM {0}.{1} WHERE email=? LIMIT 1", KEYSPACE, TABLE_NAME));
        var statement = query.Bind(email);
        return queryAccount(statement);
    }

    public Account GetAccountByID(ulong ID)
    {
        var query = Engine.DB.Session.Prepare(string.Format("SELECT id,email,name FROM {0}.{1} WHERE id=? LIMIT 1", KEYSPACE, TABLE_NAME));
        var statement = query.Bind(ID);
        return queryAccount(statement);
    }

    private Account queryAccount(BoundStatement statement)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(statement);
            var row = rs.First();
            if (row == null) throw Error.ACCOUNT_NOT_FOUND;

            var account = new Account(context);
            account.ID = row.GetValue<ulong>("id");
            account.Email = EmailAddress.Parse(row.GetValue<string>("email"));
            account.Name = Name.Create(row.GetValue<string>("name"));
            return account;
        }
        catch (scyna.Error) { throw; }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }
}