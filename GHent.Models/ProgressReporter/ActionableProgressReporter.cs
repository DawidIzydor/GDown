using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
