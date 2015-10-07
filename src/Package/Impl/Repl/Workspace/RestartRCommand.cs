using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    public sealed class RestartRCommand : MenuCommand
    {
        public RestartRCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.InteractiveSessionRestart))
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
