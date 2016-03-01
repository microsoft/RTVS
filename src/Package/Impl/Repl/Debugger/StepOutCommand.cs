using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal sealed class StepOutCommand : DebuggerWrappedCommand {
        public StepOutCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdStepOut, 
                   VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.StepOut,
                   DebuggerCommandVisibility.Stopped) {
        }

        internal override void SetStatus() {
            base.SetStatus();
            Enabled = false;
        }
    }
}
