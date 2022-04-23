using scyna.proto;

namespace scyna
{
    class Logger
    {
        public const uint INFO = 1;
        public const uint ERROR = 2;
        public const uint WARNING = 3;
        public const uint DEBUG = 4;
        public const uint FATAL = 5;
        private ulong id;
        private bool session;
        private bool enable = true;

        public Logger(ulong id, bool session)
        {
            this.id = id;
            this.session = session;
        }

        public void reset(ulong id, bool enable)
        {
            this.enable = enable;
            this.id = id;
        }


        private void add(uint level, string messgage)
        {
            messgage = format(messgage);
            Console.WriteLine(messgage);
            if (enable)
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

        public void info(string message)
        {
            add(INFO, message);
        }

        public void warning(string message)
        {
            add(WARNING, message);
        }

        public void error(string message)
        {
            add(ERROR, message);
        }

        public void debug(string message)
        {
            add(DEBUG, message);
        }

        public void fatal(string message)
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
}