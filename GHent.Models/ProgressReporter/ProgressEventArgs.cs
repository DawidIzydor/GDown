using System;

namespace GHent.Shared.ProgressReporter
{
    public class ProgressEventArgs(int done, int total, object last, ProgressType progressType, int reportedAmount, string message) : EventArgs
    {
        public int Done { get; } = done;
        public int Total { get; } = total;

        public object Last { get; } = last;
        public int ReportedAmount { get; } = reportedAmount;
        public string Message { get; } = message;
        public ProgressType ProgressType { get; } = progressType;

        public override string ToString() => $"[{ProgressType}] {Done} out of {Total}. Last reported: {Last} ({ReportedAmount}). {Message}";
    }
}
