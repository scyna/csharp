using System.ComponentModel;

namespace ex.registering;

public class Table
{
    private static string KEYSPACE = "registering";
    public static string REGISTRATION = $"{KEYSPACE}.registration";
    public static string REGISTERED = $"{KEYSPACE}.registered";
}
