using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class StopDebuggingCommand : DebuggerWrappedCommand {
        public StopDebuggingCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdStopDebugging,
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Stop, 
                   DebuggerCommandVisibility.DebugMode) {
        }
    }
}
