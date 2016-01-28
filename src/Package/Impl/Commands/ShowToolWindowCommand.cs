using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class ShowToolWindowCommand<T> : PackageCommand 
        where T : ToolWindowPane {

        public ShowToolWindowCommand(int id)
            : base(RGuidList.RCmdSetGuid, id) {}


        internal override void SetStatus() {
            Visible = true;
            Enabled = true;
        }

        internal override void Handle() {
            ToolWindowUtilities.ShowWindowPane<T>(0, true);
        }
    }
}