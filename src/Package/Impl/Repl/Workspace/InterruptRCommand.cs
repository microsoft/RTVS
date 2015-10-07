using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    public sealed class InterruptRCommand : MenuCommand
    {
        public InterruptRCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.InteractiveSessionInterrupt))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
            }
        }
    }
}
