using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHent.Shared.ProgressReporter
{
    public interface IProgressReporter<in T>
    {
        public int Total { get; }
        public int Done { get; }
        public void Report(T item);

        public void Reset(int total);
    }
}
