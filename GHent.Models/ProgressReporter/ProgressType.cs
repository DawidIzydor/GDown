#nullable enable
namespace GHent.Shared.ProgressReporter
{
    public enum ProgressType
    {
        Unknown = -1,
        Success = 1,
        Failure = 0,
        Skipped = 2,
    }
}