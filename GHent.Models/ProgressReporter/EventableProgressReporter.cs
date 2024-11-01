using System;

namespace GHent.Shared.ProgressReporter
{

    public class EventableProgressReporter : IEventableProgressReporter
    {
        public int Total { get; private set; }

        public int Done { get; private set; } = 0;

        private readonly object _lock = new();

        public event NotifyProgress OnProgress;
        public event NotifyReset OnReset;

        public void Report(ProgressType progressType, object reportedItem = null, int amount = 1, string message = null)
        {
            lock (_lock)
            {
                Done+=amount;
            }

            OnProgress?.Invoke(this, new ProgressEventArgs(Done, Total, reportedItem, progressType, amount, message));
        }

        public void Reset(int total)
        {
            lock (_lock)
            {
                Total = total;
                Done = 0;
            }

            OnReset?.Invoke(this, EventArgs.Empty);
        }

    }
}
