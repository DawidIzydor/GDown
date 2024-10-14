using System;

namespace GHent.Shared.ProgressReporter
{
    public class ActionableProgressReporter<T>(Action<IProgressReporter<T>, T> reportAction) : IProgressReporter<T>
    {
        public int Total { get; private set; }

        public int Done { get; private set; } = 0;

        private readonly object _lock = new();
        public void Report(T item)
        {
            lock (_lock)
            {
                Done++;
            }

            reportAction.Invoke(this, item);
        }

        public void Reset(int total)
        {
            lock (_lock)
            {
                Total = total;
                Done = 0;
            }
        }

        public void ReportWithDone(T item, int done)
        {
            if (done > 0)
            {
                lock (_lock)
                {
                    Done+=done;
                }
            }

            reportAction.Invoke(this, item);
        }
    }
}
