namespace Registering;

using scyna;
using Scylla.Net;

public class RepositoryBase
{
    protected Context context;
    protected const string TABLE_NAME = "user";
    protected const string KEYSPACE = "ex_registering";
    protected RepositoryBase(Context context) { this.context = context; }

    public Registration GetUserByEmail(string email)
    {
        var query = Engine.DB.Session.Prepare(string.Format("SELECT id,email,name FROM {0}.{1} WHERE email=? LIMIT 1", KEYSPACE, TABLE_NAME));
        var statement = query.Bind(email.ToString());
        return queryUser(statement);
    }

    private Registration queryUser(BoundStatement statement)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(statement);
            var row = rs.First();
            var account = new Registration
            {
                ID = (ulong)row.GetValue<long>("id"),
                Email = row.GetValue<string>("email"),
                Name = row.GetValue<string>("name")
            };
            account.ID = (ulong)row.GetValue<long>("id");
            account.Email = row.GetValue<string>("email");
            account.Name = row.GetValue<string>("name");
            return account;
        }
        catch (InvalidOperationException)
        {
            throw Error.USER_NOT_FOUND;
        }
        catch
        {
            throw scyna.Error.SERVER_ERROR;
        }
    }
}