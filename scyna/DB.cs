
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


    public void ExecuteUpdate(string query, params object[] values)
    {
        var statement = new SimpleStatement(query, values);
        ExecuteUpdate(statement);
    }

    public void ExecuteUpdate(IStatement update)
    {
        try
        {
            Engine.DB.Session.Execute(update);
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
            var rs = Engine.DB.Session.Execute(query);
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
            var rs = Engine.DB.Session.Execute(statement);
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
        var statement = new SimpleStatement(query, values);
        AssureNotExist(statement, Error.OBJECT_EXISTS);
    }

    public void AssureNotExist(IStatement query, scyna.Error existError)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(query);
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
        var statement = new SimpleStatement(query, values);
        AssureExist(statement, Error.OBJECT_NOT_FOUND);
    }

    public void AssureExist(IStatement query, scyna.Error notExistError)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(query);
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
    string AppendLimitOne(string query)
    {
        if (query.IndexOf("LIMIT") == -1 && query.IndexOf("limit") == -1) return query + " LIMIT 1";
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