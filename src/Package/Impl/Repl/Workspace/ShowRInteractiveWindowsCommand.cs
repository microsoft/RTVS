using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    public sealed class ShowRInteractiveWindowsCommand : MenuCommand
    {
        public ShowRInteractiveWindowsCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowReplWindow))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
                RPackage.Current.InteractiveWindowProvider.Open(instanceId: 0, focus: true);
            }
        }
    }
}
