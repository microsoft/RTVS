using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class StepIntoCommand : DebuggerWrappedCommand {
        public StepIntoCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdStepInto, 
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.StepInto,
                   DebuggerCommandVisibility.Stopped) {
        }
    }
}
