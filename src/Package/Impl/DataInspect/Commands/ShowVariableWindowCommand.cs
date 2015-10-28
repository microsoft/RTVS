using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ShowVariableWindowCommand : PackageCommand {
        public ShowVariableWindowCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowVariableExplorerWindow) { }

        protected override void Handle() {
            ToolWindowUtilities.ShowWindowPane<VariableWindowPane>(0, true);
        }
    }
}
