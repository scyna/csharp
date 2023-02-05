namespace scyna;

using Scylla.Net;
using Google.Protobuf;

public class Command
{
    const string TABLE_NAME = "event_store";
    static ulong version = 0;
    static string? keyspace;

    private BatchStatement batch = new BatchStatement();
    public BatchStatement Batch { get { return batch; } }

    public ulong Entity { get; set; }
    public string? Channel { get; set; }
    public IMessage? Event { get; set; }

    public void Commit(Context context)
    {
        if (version == 0)
        {
            context.Error("SingleWriter is not initialized");
            System.Environment.Exit(1);
        }

        if (Entity == 0 || Event == null) throw scyna.Error.BAD_DATA;
        var session = Engine.DB.Session;

        try
        {
            ulong id = Command.version + 1;
            var data = Event.ToByteArray();
            var query = session.Prepare(string.Format("INSERT INTO {0}.{1}(event_id, entity_id, channel, data) VALUE(?,?,?,?)", keyspace, TABLE_NAME));
            batch.Add(query.Bind(id, Entity, Channel, data));
            session.Execute(batch);
            Command.version = id;

            if (Channel != null)
            {
                var ev = new scyna.proto.Event
                {
                    TraceID = context.ID,
                    Body = ByteString.CopyFrom(data),
                    Entity = Entity,
                    Version = id,
                };
                Engine.Stream.Publish(Engine.Module + "." + Channel, ev.ToByteArray());
            }
        }
        catch { throw scyna.Error.SERVER_ERROR; }
    }

    public static void InitSingleWriter(string keyspace)
    {
        Command.keyspace = keyspace;
        var session = Engine.DB.Session;

        try
        {
            var select = session.Prepare(string.Format("SELECT max(event_id) FROM {0}.{1}", keyspace, TABLE_NAME));
            var statement = select.Bind();
            var rs = session.Execute(statement);
            var row = rs.First();
            Command.version = row.GetValue<ulong>(0);
            Console.WriteLine("Version = " + version);
        }
        catch
        {
            Console.WriteLine("Error in loading EventStore configuration");
            System.Environment.Exit(1);
        }
    }
}