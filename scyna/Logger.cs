namespace scyna;
using System.Runtime.CompilerServices;

public class Logger
{
    public const uint INFO = 1;
    public const uint ERROR = 2;
    public const uint WARNING = 3;
    public const uint DEBUG = 4;
    public const uint FATAL = 5;
    private ulong id;

    public Logger(ulong id)
    {
        this.id = id;
    }

    public ulong ID { get { return id; } }

    public void Reset(ulong id) { this.id = id; }

    private void add(uint level, string messgage)
    {
        Console.WriteLine(messgage);
        if (id > 0)
        {
            var signal = new proto.LogCreatedSignal
            {
                ID = id,
                Time = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Level = level,
                Text = messgage,
            };
            Signal.Emit(Path.LOG_CREATED_CHANNEL, signal);
        }
    }

    public void Info(string message, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        add(INFO, format(message, file, line, method));
    }

    public void Warning(string message, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        add(WARNING, format(message, file, line, method));
    }

    public void Error(string message, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        add(ERROR, format(message, file, line, method));
    }

    public void Debug(string message, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        add(DEBUG, format(message, file, line, method));
    }

    public void Fatal(string message, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        add(FATAL, format(message, file, line, method));
    }

    private string format(string log, string file, int line, string method)
    {
        return String.Format("[{0}:{1}-{2}] {3}", file, line, method, log);
    }
}
