using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands
{
    internal sealed class ShowVariableWindowCommand : PackageCommand
    {
        public ShowVariableWindowCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowVariableExplorerWindow)
        {
        }

        protected override void Handle()
        {
            ToolWindowUtilities.ShowWindowPane<VariableWindowPane>(0, true);
        }
    }
}
