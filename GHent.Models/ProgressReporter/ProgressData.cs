using System.Collections.Generic;

#nullable enable
namespace GHent.Shared.ProgressReporter
{
    public record ProgressData<T>
    {
        public required T Value { init; get; }
        public required ProgressType Type { init; get; }
        public string? Information { init; get; }
    }
}