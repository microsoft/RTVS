using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    // Identical to AttachDebugger, and only exists as a separate command so that it can be
    // given a different label for better fit in the "Debug" top-level menu.
    internal sealed class AttachToRInteractiveCommand : AttachDebuggerCommand {
        public AttachToRInteractiveCommand(IRSessionProvider rSessionProvider)
            : base(rSessionProvider, RPackageCommandId.icmdAttachToRInteractive, DebuggerCommandVisibility.DesignMode) {
        }
    }
}
