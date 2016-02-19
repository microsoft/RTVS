using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class StepOverCommand : DebuggerWrappedCommand {
        public StepOverCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdStepOver, 
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.StepOver,
                   DebuggerCommandVisibility.Stopped) {
        }
    }
}
