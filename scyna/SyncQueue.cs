using Cassandra;

namespace scyna;

public interface ISync { bool Execute(string id); }

public class SyncQueue
{
    private static readonly Dictionary<short, ISync> syncs = new();

    private static PreparedStatement appendQuery;
    private static PreparedStatement nextQuery;
    private static PreparedStatement tryToLockQuery;
    private static PreparedStatement markSyncedQuery;
    private static PreparedStatement undoQuery;

    public static void Init(string keyspace)
    {
        nextQuery = Engine.DB.Session.Prepare($@"SELECT time, id, state, locked FROM {keyspace}.sync_queue WHERE type=? LIMIT 1");
        appendQuery = Engine.DB.Session.Prepare($@"INSERT INTO {keyspace}.sync_queue(type, id, time, state) VALUES(?,?,?,0)");
        tryToLockQuery = Engine.DB.Session.Prepare($@"UPDATE {keyspace}.sync_queue SET locked=?, state=1 WHERE type=? AND id=? AND time=? IF state=0");
        markSyncedQuery = Engine.DB.Session.Prepare($@"DELETE FROM {keyspace}.sync_queue WHERE type=? AND id=? AND time = ?");
        undoQuery = Engine.DB.Session.Prepare($@"UPDATE {keyspace}.sync_queue SET state=0 WHERE type=? AND id=? AND time=?");
    }

    public static void Post(short type, string id)
    {
        try
        {
            Engine.DB.Session.Execute(appendQuery.Bind(type, id, DateTimeOffset.Now));
            Trigger(type);
        }
        catch (Exception e)
        {
            Engine.LOG.Error(e.Message);
        }
    }

    public static void Trigger(short type)
    {
        try
        {
            var sync = syncs[type];

            var rs = Engine.DB.Session.Execute(nextQuery.Bind(type));
            var row = rs.First();
            if (row is null) return;
            var id = row.GetValue<string>("id");
            var time = row.GetValue<DateTimeOffset>("time");
            var state = row.GetValue<short>("state");

            if (state == 1)
            {
                if (row.IsNull("locked")) return;
                var locked = row.GetValue<DateTimeOffset>("locked");
                if (locked.AddSeconds(15) > DateTimeOffset.Now) return;
                Engine.DB.Session.Execute(undoQuery.Bind(type, id, time));
                state = 0;
            }

            if (state != 0) return;
            if (!Engine.DB.Session.Execute(tryToLockQuery.Bind(DateTimeOffset.Now, type, id, time))
                .First().GetValue<bool>("[applied]")) { return; }

            if (sync.Execute(id))
            {
                Engine.DB.Session.Execute(markSyncedQuery.Bind(type, id, time));
            }
            else
            {
                Engine.LOG.Error($"Can not sync for type {type} => undo sync");
                Engine.DB.Session.Execute(undoQuery.Bind(type, id, time));
            }
        }
        catch (InvalidOperationException) { return; }
        catch (Exception e) { Engine.LOG.Error(e.Message); }
    }

    public static void Register(short type, ISync sync)
    {
        if (syncs.ContainsKey(type)) throw new Exception($"Sync type {type} already registered");
        syncs[type] = sync;
    }

    public static void RegisterAndOverwrite(short type, ISync sync) => syncs[type] = sync;
}