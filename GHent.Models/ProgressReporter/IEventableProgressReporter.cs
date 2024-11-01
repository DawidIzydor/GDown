using System;

namespace GHent.Shared.ProgressReporter
{
    public delegate void NotifyProgress(object sender, EventArgs e);
    public delegate void NotifyReset(object sender, EventArgs e);

    public interface IEventableProgressReporter
    {
        event NotifyProgress OnProgress;
        event NotifyReset OnReset;
        public int Total { get; }
        public int Done { get; }
        public void Report(ProgressType progressType, object reportedItem = null, int amount = 1, string message = null);
        public void Reset(int total);
    }
}