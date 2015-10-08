using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    internal sealed class InterruptRCommand : PackageCommand
    {
        public InterruptRCommand() :
            base(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.InteractiveSessionInterrupt)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
