namespace GHent.Shared.ProgressReporter
{
    public interface IProgressReporter<in T>
    {
        public int Total { get; }
        public int Done { get; }
        public void Report(T item);

        public void ReportWithDone(T item, int done);

        public void Reset(int total);
    }
}
