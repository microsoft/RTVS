using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public interface IActiveRInteractiveWindowTracker {
        IInteractiveWindowVisualComponent LastActiveWindow { get; }
        event EventHandler<InteractiveWindowChangedEventArgs> LastActiveWindowChanged;
    }
}