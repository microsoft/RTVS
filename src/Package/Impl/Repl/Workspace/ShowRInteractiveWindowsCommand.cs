using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class ShowRInteractiveWindowsCommand : PackageCommand {
        public ShowRInteractiveWindowsCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowReplWindow) {
        }

        protected override void Handle() {
            if (!ReplWindow.ReplWindowExists()) {
                var window = RPackage.Current.InteractiveWindowProvider.Create(0);
                window.Show(true);
            } else {
                ReplWindow.Show();
            }
        }
    }
}
