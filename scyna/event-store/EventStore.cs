using System.ComponentModel;
using Cassandra;
using Google.Protobuf;

namespace scyna;

public class EventStore<D> where D : IMessage<D>, new()
{
    private static EventStore<D>? instance;

    private string TableName { get; }
    private readonly List<IProjection> projections = new();
    private readonly PreparedStatement getModelQuery;
    private readonly PreparedStatement writeModelQuery;
    private readonly PreparedStatement getLastSyncedQuery;
    private readonly PreparedStatement tryToLockQuery;
    private readonly PreparedStatement markSyncedQuery;
    private readonly PreparedStatement getSyncRowQuery;

    private EventStore(string table)
    {
        TableName = table;
        getModelQuery = Engine.DB.Session.Prepare(
            $@"SELECT version,data FROM {TableName}
            WHERE id=? LIMIT 1");
        writeModelQuery = Engine.DB.Session.Prepare(
            $@"INSERT INTO {TableName}(id,type,data,event,created,version,state) 
            VALUES(?,?,?,?,?,?,0) IF NOT EXISTS");
        getLastSyncedQuery = Engine.DB.Session.Prepare(
            $@"SELECT version FROM {TableName}
            WHERE id=? AND state=2 LIMIT 1");
        tryToLockQuery = Engine.DB.Session.Prepare(
            $@"UPDATE {TableName} SET locked=?, state=1
            WHERE id=? AND version=? IF state=0");
        markSyncedQuery = Engine.DB.Session.Prepare(
            $@"UPDATE {TableName} SET state=2
            WHERE id=? AND version=?");
        getSyncRowQuery = Engine.DB.Session.Prepare(
            $@"SELECT type,data,event FROM {TableName}
            WHERE id=? AND version=? LIMIT 1");
    }

    public static EventStore<D> Instance()
    {
        if (instance is null) throw new Exception($"Event stote not initlized");
        return instance;
    }


    public static EventStore<D> New(string table)
    {
        if (instance is not null) throw new Exception($"You create EventStore for {table} twice");
        instance = new EventStore<D>(table);
        return instance;
    }

    public static void Reset() { instance = null; }
    public static string Table
    {
        get
        {
            if (instance is null) throw scyna.Error.EVENT_STORE_NULL;
            return instance.TableName;
        }
    }

    public static Model<D> ReadModel(object id)
    {
        if (instance is null) throw scyna.Error.EVENT_STORE_NULL;
        try
        {
            var rs = Engine.DB.Session.Execute(instance.getModelQuery.Bind(id));
            var row = rs.First();
            var version = row.GetValue<long>("version");

            if (row.IsNull("data")) throw scyna.Error.BAD_DATA;
            var parser = new MessageParser<D>(() => new D());
            try
            {
                var data = parser.ParseFrom(row.GetValue<byte[]>("data"));
                return new Model<D>(id, version, data, instance);
            }
            catch (Exception e)
            {
                Engine.LOG.Error(e.Message);
                throw scyna.Error.BAD_DATA;
            }
        }
        catch (InvalidOperationException)
        {
            throw scyna.Error.OBJECT_NOT_FOUND;
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    public static Model<D> CreateModel(object id)
    {
        if (instance is null) throw scyna.Error.EVENT_STORE_NULL;
        try
        {
            var rs = Engine.DB.Session.Execute(instance.getModelQuery.Bind(id));
            if (rs.Any()) throw scyna.Error.OBJECT_EXISTS;
            return new Model<D>(id, 0, new D(), instance);
        }
        catch (scyna.Error) { throw; }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            throw scyna.Error.SERVER_ERROR;
        }
    }

    internal void UpdateWriteModel(Model<D> model, IMessage event_)
    {
        try
        {
            model.Version++;
            Engine.DB.Session.Execute(writeModelQuery.Bind(
                model.ID, event_.GetType().Name, model.Data.ToByteArray(),
                event_.ToByteArray(), DateTimeOffset.Now, model.Version));
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            throw scyna.Error.COMMAND_NOT_COMPLETED;
        }
    }

    internal void PublishToEventStream(string channel, IMessage event_)
    {
        /*TODO*/
    }

    internal void UpdateReadModel(object id)
    {
        var version = GetLastSynced(id);
        if (version == -1) return;
        version++;
        while (Sync(id, version)) { version++; }
    }

    long GetLastSynced(object id)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(getLastSyncedQuery.Bind(id));
            var row = rs.First();
            if (row is null) return 0;
            return row.GetValue<long>("version");
        }
        catch (InvalidOperationException) { return 0; }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            return -1;
        }
    }

    bool Sync(object id, long version)
    {
        if (!TryToLock(id, version))
        {
            if (!LockLongLockingRow(id, version)) return false;
        }

        if (!SyncRow(id, version)) return false;
        if (!MarkSynced(id, version)) return false;
        return true;
    }

    bool TryToLock(object id, long version)
    {
        try
        {
            Engine.DB.Session.Execute(tryToLockQuery.Bind(DateTimeOffset.Now, id, version));
            return true;
        }
        catch { return false; }
    }

    bool LockLongLockingRow(object id, long version)
    {
        try
        {
            var row = Engine.DB.QueryOne($@"SELECT locked FROM {TableName}
                WHERE id=? AND version=?", id, version);
            var locked = row.GetValue<DateTimeOffset>("locked");

            if (locked.AddSeconds(5) < DateTimeOffset.Now) /*FIXME: move 10 to somewhere*/
            {
                Engine.DB.QueryOne($@"UPDATE {TableName} SET locked=? 
                    WHERE id=? AND version=? IF state=1", DateTimeOffset.Now, id, version);
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Engine.LOG.Error("LockLongLockingRow:" + e.Message);
            return false;
        }
    }

    bool SyncRow(object id, long version)
    {
        try
        {
            var rs = Engine.DB.Session.Execute(getSyncRowQuery.Bind(id, version));
            var row = rs.First();
            if (row is null) return false;

            var type = row.GetValue<string>("type");
            var data = row.GetValue<byte[]>("data");
            var event_ = row.GetValue<byte[]>("event");

            foreach (var p in projections)
            {
                if (p.Matched(type))
                {
                    p.Update(event_, data);
                    //Console.WriteLine($"Run projection = {type}");
                    return true;
                }
            }

            Console.WriteLine($"No projection for type={type}");
            /*TODO: CAN BE EXIT and ALERT to ADMIN*/

            return false;
        }
        catch (InvalidOperationException) { return false; }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            return true; /*FIXME: MUST be FALSE or ALERT to ADMIN*/
        }
    }

    bool MarkSynced(object id, long version)
    {
        try
        {
            Engine.DB.Session.Execute(markSyncedQuery.Bind(id, version));
            return true;
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
            return false;
        }
    }

    public EventStore<D> RegisterProjection<E>(Projection<E, D> projection) where E : IMessage<E>, new()
    {
        foreach (var p in projections)
        {
            if (p.Type() == projection.Type())
            {
                throw new Exception($"Type '{projection.Type}' is already registered");
            }
        }
        projections.Add(projection);
        return this;
    }

    private IMessage? ParseEvent(string type, byte[] data)
    {
        foreach (var p in projections)
        {
            if (p.Matched(type)) return p.ParseEvent(data);
        }
        return null;
    }

    public static List<EventData> ListActivity(object id, long position, int count)
    {
        if (instance is null) throw scyna.Error.EVENT_STORE_NULL;
        return instance.ListEvent(id, position, count);
    }

    private List<EventData> ListEvent(object id, long position, int count)
    {
        if (position == 0) position = Int64.MaxValue;
        if (count == 0) count = 50;
        if (count > 100) count = 100;

        var rs = Engine.DB.QueryMany($@"SELECT version,type,event,created FROM {TableName}
            WHERE id=? AND version<? LIMIT ?", id, position, count);

        var ret = new List<EventData>();

        foreach (var row in rs)
        {
            var type = row.GetValue<string>("type");
            var version = row.GetValue<long>("version");
            var data = row.GetValue<byte[]>("event");
            var event_ = ParseEvent(type, data);
            var time = row.GetValue<DateTimeOffset>("created");
            if (event_ is not null) ret.Add(new EventData(type, event_, version, time));
        }

        return ret;
    }
}