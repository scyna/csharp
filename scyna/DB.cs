
namespace scyna;
using Cassandra;
using Cassandra.Mapping;
using Google.Protobuf;

public class DB
{
    private readonly Cluster cluster;
    private readonly ISession session;
    private readonly IMapper mapper;

    private DB(String[] hosts, string username, string password)
    {
        var builder = Cluster.Builder().AddContactPoints(hosts);
        if (username.Length > 0) builder.WithCredentials(username, password);
        cluster = builder.Build();
        session = cluster.Connect();
        mapper = new Mapper(session);
    }
    public ISession Session { get { return session; } }
    public IMapper Mapper { get { return mapper; } }

    public static DB Init(String[] hosts, String username, String password)
    {
        return new DB(hosts, username, password);
    }
    public void Close()
    {
        cluster.Shutdown();
    }

    public void Execute(string query, params object[] values)
    {
        var statement = new SimpleStatement(query, values);
        try
        {
            session.Execute(statement);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public void Execute(Statement statement, params object[] values)
    {
        try
        {
            session.Execute(statement);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public void Apply(string query, params object[] values)
    {
        var statement = new SimpleStatement(query, values);
        try
        {
            if (!session.Execute(statement)
                .First().GetValue<bool>("[applied]")) throw scyna.Error.SERVER_ERROR;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public Row QueryOne(String query, params object[] values)
    {
        query = AppendLimitOne(query);
        var statement = new SimpleStatement(query, values);
        return QueryOne(statement, scyna.Error.OBJECT_NOT_FOUND);
    }

    public Row QueryOne(IStatement query, scyna.Error notfoundError)
    {
        try
        {
            var rs = session.Execute(query);
            var row = rs.First() ?? throw notfoundError;
            return row;
        }
        catch (InvalidOperationException)
        {
            throw notfoundError;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public RowSet QueryMany(String query, params object[] values)
    {
        try
        {
            var statement = new SimpleStatement(query, values);
            var rs = session.Execute(statement);
            return rs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public void AssureNotExist(String query, params object[] values)
    {
        query = AppendLimitOne(query);
        var args = values;
        var error = Error.OBJECT_EXISTS;

        if (values.Length > 0)
        {
            var last = values.Last();
            if (last is scyna.Error error1)
            {
                args = values.SkipLast(1).ToArray();
                error = error1;
            }
        }
        var statement = new SimpleStatement(query, args);
        AssureNotExist(statement, error);
    }

    private void AssureNotExist(IStatement query, scyna.Error existError)
    {
        try
        {
            var rs = session.Execute(query);
            if (rs.Any()) throw existError;
        }
        catch (scyna.Error e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public void AssureExist(String query, params object[] values)
    {
        query = AppendLimitOne(query);

        var args = values;
        var error = Error.OBJECT_NOT_FOUND;

        if (values.Length > 0)
        {
            var last = values.Last();
            if (last is scyna.Error error1)
            {
                args = values.SkipLast(1).ToArray();
                error = error1;
            }
        }

        var statement = new SimpleStatement(query, args);
        AssureExist(statement, error);
    }

    private void AssureExist(IStatement query, scyna.Error notExistError)
    {
        try
        {
            var rs = session.Execute(query);
            if (!rs.Any()) throw notExistError;
        }
        catch (scyna.Error e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    static string AppendLimitOne(string query)
    {
        if (!query.Contains("LIMIT", StringComparison.CurrentCultureIgnoreCase)) return query + " LIMIT 1";
        return query;
    }
}

public static class DBExtension
{
    public static BatchStatement Add(this BatchStatement batch, string query, params object[] values)
    {
        batch.Add(new SimpleStatement(query, values));
        return batch;
    }

    public static T? GetObject<T>(this Row row, string field) where T : class, IMessage<T>, new()
    {
        if (row.IsNull(field)) return null;

        MessageParser<T> parser = new(() => new T());
        try { return parser.ParseFrom(row.GetValue<byte[]>(field)); }
        catch { return null; }
    }

    public static T GetNotNullObject<T>(this Row row, string field) where T : class, IMessage<T>, new()
    {
        if (row.IsNull(field)) throw scyna.Error.BAD_DATA;
        MessageParser<T> parser = new(() => new T());
        try { return parser.ParseFrom(row.GetValue<byte[]>(field)); }
        catch { throw scyna.Error.BAD_DATA; }
    }
}