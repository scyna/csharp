namespace ex.registering;

public class Table
{
    private static readonly string KEYSPACE = "registering";
    public static string REGISTRATION = $"{KEYSPACE}.registration";
    public static string COMPLETED = $"{KEYSPACE}.completed";
}
