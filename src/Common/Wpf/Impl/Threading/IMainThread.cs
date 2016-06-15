using System;

namespace Microsoft.Common.Wpf.Threading {
    public interface IMainThread {
        int ThreadId { get; }
        void Post(Action action);
    }
}