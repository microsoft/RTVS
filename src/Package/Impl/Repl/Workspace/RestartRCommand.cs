using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    internal sealed class RestartRCommand : PackageCommand
    {
        public RestartRCommand() :
            base(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.InteractiveSessionRestart)
        {
        }

        protected override void SetStatus()
        {
            this.Enabled = ReplWindow.ReplWindowExists();
        }
    }
}
