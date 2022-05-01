using scyna.proto;

namespace scyna;

public class Logger
{
    public const uint INFO = 1;
    public const uint ERROR = 2;
    public const uint WARNING = 3;
    public const uint DEBUG = 4;
    public const uint FATAL = 5;
    private ulong id;
    private bool session;

    public Logger(ulong id, bool session)
    {
        this.id = id;
        this.session = session;
    }

    public void Reset(ulong id)
    {
        this.id = id;
    }


    private void add(uint level, string messgage)
    {
        messgage = format(messgage);
        Console.WriteLine(messgage);
        if (id > 0)
        {
            var signal = new proto.WriteLogSignal
            {
                Id = id,
                Time = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Level = level,
                Text = messgage,
                Session = session,
            };
            Signal.Emit(Path.LOG_WRITE_CHANNEL, signal);
        }
    }

    public void Info(string message)
    {
        add(INFO, message);
    }

    public void Warning(string message)
    {
        add(WARNING, message);
    }

    public void Error(string message)
    {
        add(ERROR, message);
    }

    public void Debug(string message)
    {
        add(DEBUG, message);
    }

    public void Fatal(string message)
    {
        add(FATAL, message);
    }

    private string format(string log)
    {
        // StackTraceElement[] elements = Thread.currentThread().getStackTrace();
        // String newLog = "[" + elements[4].getFileName() + ":" + elements[4].getLineNumber() + " - "
        //         + elements[4].getMethodName() + "] " + log;
        return log;
    }
}
