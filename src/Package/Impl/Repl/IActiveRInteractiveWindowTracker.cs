using System;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IActiveRInteractiveWindowTracker {
        IInteractiveWindowVisualComponent LastActiveWindow { get; }
        event EventHandler<InteractiveWindowChangedEventArgs> LastActiveWindowChanged;
    }
}