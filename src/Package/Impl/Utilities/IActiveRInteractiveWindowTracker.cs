using System;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public interface IActiveRInteractiveWindowTracker {
        IInteractiveWindow LastActiveWindow { get; }
        event EventHandler<InteractiveWindowChangedEventArgs> LastActiveWindowChanged;
    }
}