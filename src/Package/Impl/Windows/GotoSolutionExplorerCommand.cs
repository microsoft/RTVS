// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Windows {
    internal sealed class GotoSolutionExplorerCommand : PackageCommand {
        public GotoSolutionExplorerCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowSolutionExplorer) {
        }

        protected override void SetStatus() {
            Supported = Enabled = true;
        }

        protected override void Handle() {
            var uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            object o = new object();
            uiShell.PostExecCommand(typeof(VSConstants.VSStd97CmdID).GUID, (uint)VSConstants.VSStd97CmdID.ProjectExplorer, 0, ref o);
        }
    }
}
