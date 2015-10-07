using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Commands.Global
{
    public sealed class GoToOptionsCommand: MenuCommand
    {
        public GoToOptionsCommand():
            base((sender, args) => new Handler().OnCommand(), 
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGoToRToolsOptions))
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
                    ((Microsoft.VisualStudio.Shell.Package)package).ShowOptionPage(typeof(RToolsOptionsPage));
                }
            }
        }
    }
}
