using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class StopDebuggingCommand : DebuggerWrappedCommand {
        public StopDebuggingCommand(IRSessionProvider rSessionProvider)
            : base(rSessionProvider, RPackageCommandId.icmdStopDebugging,
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Stop, 
                   DebuggerCommandVisibility.DebugMode) {
        }
    }
}
