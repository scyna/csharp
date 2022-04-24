namespace scyna
{
    class Session
    {
        public ulong ID { get; }
        private ulong sequence;

        public Session(ulong id)
        {
            this.ID = id;
            var timer = new System.Timers.Timer(1000 * 60 * 10);
            timer.Elapsed += OnUpdate;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public ulong Sequence
        {
            get
            {
                lock (this)
                {
                    sequence++;
                    return sequence;
                }
            }
        }

        private static void OnUpdate(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Update Session");
            /*TODO: send update signal to Manager*/
        }
    }
}