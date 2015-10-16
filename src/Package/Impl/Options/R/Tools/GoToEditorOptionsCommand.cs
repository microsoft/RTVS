using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools
{
    public sealed class GoToEditorOptionsCommand : MenuCommand
    {
        public GoToEditorOptionsCommand():
            base((sender, args) => new Handler().OnCommand(), 
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGoToREditorOptions))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
                IVsShell shell = AppShell.Current.GetGlobalService<IVsShell>(typeof(SVsShell));
                IVsPackage package;

                if (VSConstants.S_OK == shell.LoadPackage(RGuidList.RPackageGuid, out package))
                {
                    ((Microsoft.VisualStudio.Shell.Package)package).ShowOptionPage(typeof(REditorOptionsDialog));
                }
            }
        }
    }
}
