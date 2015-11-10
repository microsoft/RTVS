using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class ShowHelpWindowCommand : PackageCommand {
        public ShowHelpWindowCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowHelpWindow) { }

        protected override void SetStatus() {
            Visible = true;
            Enabled = true;
        }

        protected override void Handle() {
            ToolWindowUtilities.ShowWindowPane<HelpWindowPane>(0, true);
        }
    }
}
