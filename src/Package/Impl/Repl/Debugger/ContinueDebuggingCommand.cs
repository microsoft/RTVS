using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class ContinueDebuggingCommand : DebuggerWrappedCommand {
        public ContinueDebuggingCommand(IRSessionProvider rSessionProvider)
            : base(rSessionProvider, RPackageCommandId.icmdContinueDebugging, 
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Start, 
                   DebuggerCommandVisibility.Stopped) {
        }
    }
}
