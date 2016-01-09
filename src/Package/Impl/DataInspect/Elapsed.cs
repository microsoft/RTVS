//#define PRINT_ELAPSED

using System;
using System.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Prints elapsed time, simple development utility calss
    /// </summary>
    internal class Elapsed : IDisposable {
        Stopwatch _watch;
        string _header;
        public Elapsed(string header) {
#if DEBUG && PRINT_ELAPSED
            _header = header;
            _watch = Stopwatch.StartNew();
#endif
        }

        public void Dispose() {
#if DEBUG && PRINT_ELAPSED
            Trace.WriteLine(_header + _watch.ElapsedMilliseconds);
#endif
        }
    }
}
